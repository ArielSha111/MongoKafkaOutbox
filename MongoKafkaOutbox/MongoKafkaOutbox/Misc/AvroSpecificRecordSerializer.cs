using Avro.IO;
using Avro.Specific;
using Avro;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using System.Net;
using Encoder = Avro.IO.Encoder;
using MongoKafkaOutbox.Misc;

public class AvroSpecificRecordSerializer<T> : IAvroSerializer<T>
{
    private ISchemaRegistryClient schemaRegistryClient;
    private bool autoRegisterSchema;
    private bool normalizeSchemas;
    private bool useLatestVersion;
    private int initialBufferSize;
    private SubjectNameStrategyDelegate subjectNameStrategy;
    private Dictionary<System.Type, AvroSpecificRecordSerializer<T>.SerializerSchemaData> multiSchemaData = new Dictionary<System.Type, AvroSpecificRecordSerializer<T>.SerializerSchemaData>();
    private AvroSpecificRecordSerializer<T>.SerializerSchemaData singleSchemaData;
    private SemaphoreSlim serializeMutex = new SemaphoreSlim(1);

    public AvroSpecificRecordSerializer(
      ISchemaRegistryClient schemaRegistryClient,
      bool autoRegisterSchema,
      bool normalizeSchemas,
      bool useLatestVersion,
      int initialBufferSize,
      SubjectNameStrategyDelegate subjectNameStrategy)
    {
        this.schemaRegistryClient = schemaRegistryClient;
        this.autoRegisterSchema = autoRegisterSchema;
        this.normalizeSchemas = normalizeSchemas;
        this.useLatestVersion = useLatestVersion;
        this.initialBufferSize = initialBufferSize;
        this.subjectNameStrategy = subjectNameStrategy;
        System.Type writerType = typeof(T);
        if (!(writerType != typeof(ISpecificRecord)))
            return;
        this.singleSchemaData = AvroSpecificRecordSerializer<T>.ExtractSchemaData(writerType);
    }

    private static AvroSpecificRecordSerializer<T>.SerializerSchemaData ExtractSchemaData(System.Type writerType)
    {
        AvroSpecificRecordSerializer<T>.SerializerSchemaData schemaData = new AvroSpecificRecordSerializer<T>.SerializerSchemaData();
        if (typeof(ISpecificRecord).IsAssignableFrom(writerType))
            schemaData.WriterSchema = ((ISpecificRecord)Activator.CreateInstance(writerType)).Schema;
        else if (writerType.Equals(typeof(int)))
            schemaData.WriterSchema = Avro.Schema.Parse("int");
        else if (writerType.Equals(typeof(bool)))
            schemaData.WriterSchema = Avro.Schema.Parse("boolean");
        else if (writerType.Equals(typeof(double)))
            schemaData.WriterSchema = Avro.Schema.Parse("double");
        else if (writerType.Equals(typeof(string)))
            schemaData.WriterSchema = Avro.Schema.Parse("string");
        else if (writerType.Equals(typeof(float)))
            schemaData.WriterSchema = Avro.Schema.Parse("float");
        else if (writerType.Equals(typeof(long)))
            schemaData.WriterSchema = Avro.Schema.Parse("long");
        else if (writerType.Equals(typeof(byte[])))
        {
            schemaData.WriterSchema = Avro.Schema.Parse("bytes");
        }
        else
        {
            if (!writerType.Equals(typeof(Null)))
                throw new InvalidOperationException("AvroSerializer only accepts type parameters of int, bool, double, string, float, long, byte[], instances of ISpecificRecord and subclasses of SpecificFixed.");
            schemaData.WriterSchema = Avro.Schema.Parse("null");
        }
        schemaData.AvroWriter = new SpecificWriter<T>(schemaData.WriterSchema);
        schemaData.WriterSchemaString = ((object)schemaData.WriterSchema).ToString();
        return schemaData;
    }

    public async Task<byte[]> Serialize(string topic, T data, bool isKey)
    {
        byte[] array;
        try
        {
            await this.serializeMutex.WaitAsync().ConfigureAwait(false);
            AvroSpecificRecordSerializer<T>.SerializerSchemaData currentSchemaData;
            try
            {
                if (this.singleSchemaData == null)
                {
                    System.Type type = data.GetType();
                    if (!this.multiSchemaData.TryGetValue(type, out currentSchemaData))
                    {
                        currentSchemaData = AvroSpecificRecordSerializer<T>.ExtractSchemaData(type);
                        this.multiSchemaData[type] = currentSchemaData;
                    }
                }
                else
                    currentSchemaData = this.singleSchemaData;
                string recordType = (string)null;
                if ((object)data is ISpecificRecord && ((ISpecificRecord)(object)data).Schema is RecordSchema)
                    recordType = ((ISpecificRecord)(object)data).Schema.Fullname;
                string subject = this.subjectNameStrategy != null ? this.subjectNameStrategy(new SerializationContext(isKey ? MessageComponentType.Key : MessageComponentType.Value, topic), recordType) : (isKey ? this.schemaRegistryClient.ConstructKeySubjectName(topic, recordType) : this.schemaRegistryClient.ConstructValueSubjectName(topic, recordType));
                if (!currentSchemaData.SubjectsRegistered.Contains(subject))
                {
                    if (this.useLatestVersion)
                    {
                        currentSchemaData.WriterSchemaId = new int?((await this.schemaRegistryClient.GetLatestSchemaAsync(subject).ConfigureAwait(false)).Id);
                    }
                    else
                    {
                        AvroSpecificRecordSerializer<T>.SerializerSchemaData serializerSchemaData = currentSchemaData;
                        int num;
                        if (this.autoRegisterSchema)
                            num = await this.schemaRegistryClient.RegisterSchemaAsync(subject, currentSchemaData.WriterSchemaString, this.normalizeSchemas).ConfigureAwait(false);
                        else
                            num = await this.schemaRegistryClient.GetSchemaIdAsync(subject, currentSchemaData.WriterSchemaString, this.normalizeSchemas).ConfigureAwait(false);
                        serializerSchemaData.WriterSchemaId = new int?(num);
                        serializerSchemaData = (AvroSpecificRecordSerializer<T>.SerializerSchemaData)null;
                    }
                    currentSchemaData.SubjectsRegistered.Add(subject);
                }
                subject = (string)null;
            }
            finally
            {
                this.serializeMutex.Release();
            }
            using (MemoryStream output = new MemoryStream(this.initialBufferSize))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter((Stream)output))
                {
                    output.WriteByte((byte)0);
                    binaryWriter.Write(IPAddress.HostToNetworkOrder(currentSchemaData.WriterSchemaId.Value));
                    currentSchemaData.AvroWriter.Write(data, (Encoder)new BinaryEncoder((Stream)output));
                    array = output.ToArray();
                }
            }
        }
        catch (AggregateException ex)
        {
            throw ex.InnerException;
        }
        return array;
    }

    public class SerializerSchemaData
    {
        private string writerSchemaString;
        private Avro.Schema writerSchema;
        /// <remarks>
        ///     A given schema is uniquely identified by a schema id, even when
        ///     registered against multiple subjects.
        /// </remarks>
        private int? writerSchemaId;
        private SpecificWriter<T> avroWriter;
        private HashSet<string> subjectsRegistered = new HashSet<string>();

        public HashSet<string> SubjectsRegistered
        {
            get => this.subjectsRegistered;
            set => this.subjectsRegistered = value;
        }

        public string WriterSchemaString
        {
            get => this.writerSchemaString;
            set => this.writerSchemaString = value;
        }

        public Avro.Schema WriterSchema
        {
            get => this.writerSchema;
            set => this.writerSchema = value;
        }

        public int? WriterSchemaId
        {
            get => this.writerSchemaId;
            set => this.writerSchemaId = value;
        }

        public SpecificWriter<T> AvroWriter
        {
            get => this.avroWriter;
            set => this.avroWriter = value;
        }
    }
}

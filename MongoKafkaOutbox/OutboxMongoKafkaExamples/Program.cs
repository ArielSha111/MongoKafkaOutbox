using System.Text;
using Avro.IO;
using Avro.Specific;
using Confluent.Kafka;

namespace AvroSchema;

public class Program
{
    public static void Main()
    {
        var p = new Person();
        Console.WriteLine(p.Age + p.Name);
        Console.WriteLine(p.Schema);

        var singleSchemaData = ExtractSchemaData(p);
        var byteArr = Serialize(p, singleSchemaData);

        Console.WriteLine(singleSchemaData);
        var deserialized = Deserialize(byteArr, singleSchemaData);
        Console.WriteLine(deserialized.Age + " " + deserialized.Name);
    }

    private static AvroSpecificRecordSerializer<T>.SerializerSchemaData ExtractSchemaData<T>(T data)
    {
        var writerType = typeof(Person);
        var schemaData = new AvroSpecificRecordSerializer<T>.SerializerSchemaData();
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
        schemaData.WriterSchemaString = schemaData.WriterSchema.ToString();
        return schemaData;
    }




    private static byte[] Serialize<T>(T data, AvroSpecificRecordSerializer<T>.SerializerSchemaData singleSchemaData)
    {
        using var output = new MemoryStream(1024);
        using var binaryWriter = new BinaryWriter(output);
        binaryWriter.Write(Encoding.UTF8.GetBytes(typeof(T).FullName));
        singleSchemaData.AvroWriter.Write(data, new BinaryEncoder(output));
        var array = output.ToArray();
        return array;
    }

    private static T Deserialize<T>(byte[] data, AvroSpecificRecordSerializer<T>.SerializerSchemaData singleSchemaData)
    {
        var start = (Encoding.UTF8.GetBytes(typeof(T).FullName)).Length;

        var subArrayLength = data.Length - start;
        var subArray = new byte[subArrayLength];
        Array.Copy(data, start, subArray, 0, subArrayLength);

        using var input = new MemoryStream(subArray);
        using var avroStream = new MemoryStream(subArray);
        var datumReader = new SpecificReader<T>(singleSchemaData.WriterSchema, singleSchemaData.WriterSchema);
        var decoder = new BinaryDecoder(avroStream);
        return datumReader.Read(default, decoder);
    }
}

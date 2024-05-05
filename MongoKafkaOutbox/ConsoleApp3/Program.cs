using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using ConsoleApp3;
using Confluent.Kafka.SyncOverAsync;
using Avro.Generic;
using Avro.IO;
using Avro;
class Program
{
    const string bootstrapServers = "localhost:19092";
    const string schemaRegistryUrl = "http://localhost:8081";
    const string topicName = "avro-topic";
    const string consumerGroup = "avro-cg-001";

    static async Task Main()
    {
        string avroSchemaJson = @"{
    ""type"": ""record"",
    ""name"": ""MyObject"",
    ""fields"": [
        { ""name"": ""Field1"", ""type"": ""string"" },
        { ""name"": ""Field2"", ""type"": ""int"" }
    ]
}";

        var schema = (RecordSchema)Avro.Schema.Parse(avroSchemaJson);

        var avroBytes = PrintAvroMessage(schema);
        ReadAvroMessage(schema, avroBytes);

        StartProducer();
        StartConsumer();
        Console.ReadLine();
    }
    private static byte[] PrintAvroMessage(RecordSchema schema)
    {
        var genericRecord = new GenericRecord(schema);

        var myObject = new
        {
            Field1 = "Ariel",
            Field2 = 123
        };

        genericRecord.Add("Field1", myObject.Field1);
        genericRecord.Add("Field2", myObject.Field2);

        using var stream = new MemoryStream() ;
        var encoder = new BinaryEncoder(stream);
        var writer = new GenericDatumWriter<GenericRecord>(schema);
        writer.Write(genericRecord, encoder);
        encoder.Flush();

        byte[] avroBytes = stream.ToArray();
        string avroString = BitConverter.ToString(avroBytes).Replace("-", "");
        Console.WriteLine(avroString);
        return avroBytes;
        
    }
    private static void ReadAvroMessage(RecordSchema schema, byte[] avroBytes)
    {
        using var stream = new MemoryStream(avroBytes);
        var decoder = new BinaryDecoder(stream);
        var reader = new GenericDatumReader<GenericRecord>(schema, schema);
        var genericRecord = reader.Read(null, decoder);

        // Extract values from the generic record
        string field1 = (string)genericRecord.GetValue(0);
        int field2 = (int)genericRecord.GetValue(1);

        // Construct your object
        var myObject = new 
        {
            Field1 = field1,
            Field2 = field2
        };

    }

   

    private static async Task StartProducer()
    {
       var producerConfig = new ProducerConfig
        {
            BootstrapServers = bootstrapServers
        };

        var schemaRegistryConfig = new SchemaRegistryConfig
        {
            Url = schemaRegistryUrl
        };

        var schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig);


        var producer =
             new ProducerBuilder<string, Person>(producerConfig)
                .SetValueSerializer(new AvroSerializer<Person>(schemaRegistry))
                .Build();


        while (true) 
        {
            var person = new Person
            {
                Age = 1,
                Name = "some name"
            };

            Console.WriteLine($"Sending message with person");

            await producer.ProduceAsync(topicName, new Message<string, Person> { Value = person });

            await Task.Delay(2000);
        }

        producer.Dispose();
        schemaRegistry.Dispose();
    }

    private static Task StartConsumer()
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = consumerGroup
        };

        var schemaRegistryConfig = new SchemaRegistryConfig
        {
            Url = schemaRegistryUrl
        };

        var schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig);

        var consumer =
                new ConsumerBuilder<string, Person>(consumerConfig)
                    .SetValueDeserializer(new AvroDeserializer<Person>(schemaRegistry).AsSyncOverAsync())
                    .SetErrorHandler((_, e) => Console.WriteLine($"Error: {e.Reason}"))
                    .Build();

        consumer.Subscribe(topicName);


        while (true)
        {
            try
            {
                var cr = consumer.Consume();
                var message = cr.Message.Value;
                Console.WriteLine($"Receiving message with person");
            }
            catch (ConsumeException e)
            {
                Console.WriteLine(e);
            }
        }

        consumer.Close();
        schemaRegistry.Dispose();
    }
}

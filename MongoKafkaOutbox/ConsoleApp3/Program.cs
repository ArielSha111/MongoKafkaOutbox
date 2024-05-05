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
        //        string avroSchemaJson = @"{
        //    ""type"": ""record"",
        //    ""name"": ""Person"",
        //    ""fields"": [
        //        { ""name"": ""Age"", ""type"": ""int"" },
        //        { ""name"": ""Name"", ""type"": ""string"" }
        //    ]
        //}";

        //        var schema = (RecordSchema)Avro.Schema.Parse(avroSchemaJson);

        //        var avroBytes = PrintAvroMessageBytes(schema);
        //        ReadAvroMessage(schema, avroBytes);

        StartProducer();
        StartConsumer();
        Console.ReadLine();
    }

    private static string GetAvroMessageString(RecordSchema schema)
    {
        var avroBytes = PrintAvroMessageBytes(schema);
        string avroString = BitConverter.ToString(avroBytes).Replace("-", "");
        return avroString;
    }

    private static byte[] PrintAvroMessageBytes(RecordSchema schema)
    {
        var genericRecord = new GenericRecord(schema);

        var person = new Person
        {
            Age = 1,
            Name = "some name"
        };


        genericRecord.Add("Age", person.Age);
        genericRecord.Add("Name", person.Name);

        using var stream = new MemoryStream() ;
        var encoder = new BinaryEncoder(stream);
        var writer = new GenericDatumWriter<GenericRecord>(schema);
        writer.Write(genericRecord, encoder);
        encoder.Flush();

        byte[] avroBytes = stream.ToArray();
        string avroString = BitConverter.ToString(avroBytes).Replace("-", "");
        return avroBytes;
    }

    private static void ReadAvroMessage(RecordSchema schema, byte[] avroBytes)
    {
        using var stream = new MemoryStream(avroBytes);
        var decoder = new BinaryDecoder(stream);
        var reader = new GenericDatumReader<GenericRecord>(schema, schema);
        var genericRecord = reader.Read(null, decoder);

        int age = (int)genericRecord.GetValue(0);
        string name = (string)genericRecord.GetValue(1);

        var person = new Person
        {
            Age = 1,
            Name = "some name"
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

            var schema = (RecordSchema)Avro.Schema.Parse(person.Schema.ToString());
            var message = GetAvroMessageString(schema);

            await producer.ProduceAsync(topicName, new Message<string, Person> { Value = person });
            Console.WriteLine($"Sent message with person {message}");

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
                new ConsumerBuilder<string, byte[]>(consumerConfig)
                    .SetErrorHandler((_, e) => Console.WriteLine($"Error: {e.Reason}"))
                    .Build();


        //var consumer =
        //        new ConsumerBuilder<string, Person>(consumerConfig)
        //            .SetValueDeserializer(new AvroDeserializer<Person>(schemaRegistry).AsSyncOverAsync())
        //            .SetErrorHandler((_, e) => Console.WriteLine($"Error: {e.Reason}"))
        //            .Build();


        consumer.Subscribe(topicName);


        while (true)
        {
            try
            {
                var cr = consumer.Consume();
                var avroBytes = cr.Message.Value;
                string avroString = BitConverter.ToString(avroBytes).Replace("-", "");
                Console.WriteLine($"Read message with person {avroString}");
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

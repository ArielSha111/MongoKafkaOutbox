using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using ConsoleApp3;
using Confluent.Kafka.SyncOverAsync;
class Program
{
    const string bootstrapServers = "localhost:19092";
    const string schemaRegistryUrl = "http://localhost:8081";
    const string topicName = "avro-topic";
    const string consumerGroup = "avro-cg-001";

    static async Task Main()
    {   
        StartProducer();
        StartConsumer();
        Console.ReadLine();
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

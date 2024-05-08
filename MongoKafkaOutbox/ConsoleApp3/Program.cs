using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using ConsoleApp3;
using Confluent.Kafka.SyncOverAsync;

class Program
{
    const string bootstrapServers = "localhost:19092";
    const string schemaRegistryUrl = "http://localhost:8081";
    const string topicName = "my-topic4";
    const string consumerGroup = "my_consumer_group";

    static async Task Main()
    {
        var schemaRegistryConfig = new SchemaRegistryConfig
        {
            Url = schemaRegistryUrl
        };

        using var schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig);


        var a = StartProducer(schemaRegistry);
        var b = StartConsumer(schemaRegistry);
        Task.WaitAll(b);

        Console.ReadLine();
    }

    private static async Task StartProducer(CachedSchemaRegistryClient schemaRegistry)
    {
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = bootstrapServers
        };


        var serializer = new AvroSerializer<Person>(schemaRegistry);

        using var producer = new ProducerBuilder<Null, byte[]>(producerConfig).Build();
        var serializationContext = new SerializationContext(MessageComponentType.Value, topicName);
        while (true)
        {
            try
            {

                var person = new Person
                {
                    Age = 13,
                    Name = Guid.NewGuid().ToString(),
                };

                var serializedBytes = await serializer.SerializeAsync(person, serializationContext);
                var result = await producer.ProduceAsync(topicName, new Message<Null, byte[]> { Value = serializedBytes });
                Console.WriteLine($"produce message with person: {person.Name}, {BitConverter.ToString(result.Message.Value).Replace("-", "")}");
                await Task.Delay(100);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

    }

    private static async Task StartConsumer(CachedSchemaRegistryClient schemaRegistry)
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = consumerGroup,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };


        using var consumer = new ConsumerBuilder<Null, Person>(consumerConfig)
                    .SetValueDeserializer(new AvroDeserializer<Person>(schemaRegistry).AsSyncOverAsync())
                    .SetErrorHandler((_, e) => Console.WriteLine($"Error: {e.Reason}"))
                    .Build();


        consumer.Subscribe(topicName);


        while (true)
        {
            try
            {
                var cr = consumer.Consume();
                var person = cr.Message.Value;
                Console.WriteLine($"Consumed message with person: {person.Name}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}

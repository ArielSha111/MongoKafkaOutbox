using Confluent.Kafka;
using Confluent.SchemaRegistry;

namespace MongoKafkaOutbox.Messaging;

public class KafkaService
{
    private readonly string _bootstrapServers;
    private readonly ISchemaRegistryClient _schemaRegistryClient;

    public KafkaService()
    {
        _bootstrapServers = "localhost:9092";
        _schemaRegistryClient = new CachedSchemaRegistryClient(
            new SchemaRegistryConfig 
            { 
                Url = "http://localhost:8081"
            });
    }

    public async Task ProduceMessageAsync()
    {
        var config = new ProducerConfig
        {
            BootstrapServers = _bootstrapServers
        };

        using (var producer = new ProducerBuilder<Null, string>(config).Build())
        {
            try
            {
                while (true)
                {
                    var message = "some message";
                    var result = await producer.ProduceAsync("", new Message<Null, string> { Value = message });
                    Console.WriteLine($"Produced message '{message}' to topic {result.Topic}, partition {result.Partition}, offset {result.Offset}");
                }
            }
            catch (ProduceException<Null, string> ex)
            {
                Console.WriteLine($"Delivery failed: {ex.Error.Reason}");
            }
        }
    }

    //public async Task ProduceAvroMessageAsync(string topic, string sessionId, object data)
    //{
    //    var avroSerializer = new AvroSerializer<object>(_schemaRegistryClient);
    //    var serializationContext = new SerializationContext(MessageComponentType.Value, "my_topic");

    //    var serializedData = await avroSerializer.SerializeAsync(data, serializationContext);

    //    var config = new ProducerConfig { BootstrapServers = _bootstrapServers };

    //    using (var producer = new ProducerBuilder<string, byte[]>(config).Build())
    //    {
    //        await producer.ProduceAsync(topic, new Message<string, byte[]> { Key = sessionId, Value = serializedData });
    //    }
    //}
}

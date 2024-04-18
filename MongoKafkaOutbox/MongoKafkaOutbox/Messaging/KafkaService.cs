using Confluent.SchemaRegistry.Serdes;
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

    public async Task ProduceAvroMessageAsync(string topic, string sessionId, object data)
    {
        var avroSerializer = new AvroSerializer<object>(_schemaRegistryClient);
        var serializationContext = new SerializationContext(MessageComponentType.Value, "my_topic");

        var serializedData = await avroSerializer.SerializeAsync(data, serializationContext);

        var config = new ProducerConfig { BootstrapServers = _bootstrapServers };

        using (var producer = new ProducerBuilder<string, byte[]>(config).Build())
        {
            await producer.ProduceAsync(topic, new Message<string, byte[]> { Key = sessionId, Value = serializedData });
        }
    }
}

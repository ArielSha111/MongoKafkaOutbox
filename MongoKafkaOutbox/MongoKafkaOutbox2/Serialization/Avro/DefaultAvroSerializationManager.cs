
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;

namespace MongoKafkaOutbox2.Serialization.Avro;

public class DefaultAvroSerializationManager(CachedSchemaRegistryClient schemaRegistry) : IAvroSerializationManager
{
    public async Task<byte[]> GetAsAvroAsync<T>(T message)
    {
        var serializationContext = new SerializationContext(MessageComponentType.Value, "topic_name");
        var serializer = new AvroSerializer<T>(schemaRegistry);
        var serializedBytes = await serializer.SerializeAsync(message, serializationContext);
        return serializedBytes;
    }
}

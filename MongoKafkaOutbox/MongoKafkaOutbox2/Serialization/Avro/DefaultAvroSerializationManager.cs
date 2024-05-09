
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;

namespace MongoKafkaOutbox2.Serialization.Avro;

internal class DefaultAvroSerializationManager(CachedSchemaRegistryClient schemaRegistry, string kafkaTopicName) : IAvroSerializationManager
{
    public async Task<byte[]> GetAsAvroAsync<T>(T message)
    {
        var serializationContext = new SerializationContext(MessageComponentType.Value, kafkaTopicName);
        var serializer = new AvroSerializer<T>(schemaRegistry);
        var serializedBytes = await serializer.SerializeAsync(message, serializationContext);
        return serializedBytes;
    }
}

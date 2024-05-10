
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using MongoKafkaOutbox.Contracts;

namespace MongoKafkaOutbox.Serialization.Avro;

internal class DefaultAvroSerializationManager(SchemaRegistryConfig schemaRegistryConfig, OutboxConfigurationBlock outboxConfigurationBlock) : IAvroSerializationManager
{
    public async Task<byte[]> GetAsAvroAsync<T>(T message)
    {
        using var schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig);
        var serializationContext = new SerializationContext(MessageComponentType.Value, outboxConfigurationBlock.kafkaTopicName);
        var serializer = new AvroSerializer<T>(schemaRegistry);
        var serializedBytes = await serializer.SerializeAsync(message, serializationContext);
        return serializedBytes;
    }
}

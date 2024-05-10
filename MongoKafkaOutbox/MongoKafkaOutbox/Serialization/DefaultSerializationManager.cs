using MongoKafkaOutbox.Serialization.Avro;

namespace MongoKafkaOutbox.Serialization;

internal class DefaultSerializationManager(IAvroSerializationManager avroSerializationManager) : ISerializationManager
{
    public async Task<object> SerializeAsync<T>(T message)
    {
        return await avroSerializationManager.GetAsAvroAsync(message);
    }
}

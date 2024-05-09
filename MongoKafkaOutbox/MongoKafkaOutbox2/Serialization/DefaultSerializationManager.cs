using MongoKafkaOutbox2.Serialization.Avro;

namespace MongoKafkaOutbox2.Serialization;

public class DefaultAvroSerializationManager(IAvroSerializationManager avroSerializationManager) : ISerializationManager
{
    public async Task<object> SerializeAsync<T>(T message)
    {
        return await avroSerializationManager.GetAsAvroAsync(message);
    }
}

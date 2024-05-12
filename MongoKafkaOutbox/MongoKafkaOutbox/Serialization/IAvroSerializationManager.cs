namespace MongoKafkaOutbox.Serialization;

public interface IAvroSerializationManager
{
    public Task<byte[]> GetAsAvroAsync<T>(T message);
}

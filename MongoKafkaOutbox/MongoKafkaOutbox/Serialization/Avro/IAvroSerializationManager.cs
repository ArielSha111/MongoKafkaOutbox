namespace MongoKafkaOutbox.Serialization.Avro;

public interface IAvroSerializationManager
{
    public Task<byte[]> GetAsAvroAsync<T>(T message);
}

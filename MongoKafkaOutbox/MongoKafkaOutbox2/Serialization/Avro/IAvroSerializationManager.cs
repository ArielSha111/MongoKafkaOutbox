namespace MongoKafkaOutbox2.Serialization.Avro;

public interface IAvroSerializationManager
{
    public Task<byte[]> GetAsAvroAsync<T>(T message);
}

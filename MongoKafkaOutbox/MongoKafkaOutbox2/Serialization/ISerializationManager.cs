namespace MongoKafkaOutbox2.Serialization;

public interface ISerializationManager
{
    public Task<object> SerializeAsync<T>(T message);
}

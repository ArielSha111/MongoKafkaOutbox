namespace MongoKafkaOutbox.Serialization;

public interface ISerializationManager
{
    public Task<object> SerializeAsync<T>(T message);
}

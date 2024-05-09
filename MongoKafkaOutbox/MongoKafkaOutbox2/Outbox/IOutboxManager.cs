
namespace MongoKafkaOutbox2.Outbox;

public interface IOutboxManager
{
    public Task PublishMessage<T>(T message);
    public Task<IOutboxClientSessionHandle> StartSessionAsync();
}
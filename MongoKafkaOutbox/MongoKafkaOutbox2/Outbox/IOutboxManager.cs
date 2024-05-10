using Avro.Specific;
using MongoDB.Driver;

namespace MongoKafkaOutbox2.Outbox;

public interface IOutboxManager
{
    public Task<IClientSessionHandle> StartOutboxSessionAsync();

    public Task PublishMessage<T>(T message) where T : ISpecificRecord;
}
using MongoDB.Driver;

namespace MongoKafkaOutbox.Outbox.Default;

public interface IGenericOutboxManager<T> : IOutboxManager
{
    public IMongoCollection<T> Collection { get; set; }
}
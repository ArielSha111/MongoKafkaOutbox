using MongoDB.Driver;

namespace MongoKafkaOutbox2.Outbox.Default;

public interface IGenericOutboxManager<T> : IOutboxManager
{
    public IMongoCollection<T> Collection { get; set; }
}
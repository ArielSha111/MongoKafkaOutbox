using MongoDB.Bson;
using MongoDB.Driver;
using MongoKafkaOutbox.Outbox;

namespace MongoKafkaOutbox.Mongo;

public interface IMongoDBService<T>
{
    public abstract IMongoCollection<T> Collection { get; set; }
    public abstract IMongoCollection<OutboxEvent> OutboxCollection { get; set; }

    public Task AddToBothCollectionsWithTransaction(Func<Task> mainTask, Func<Task> outboxTask);
    public Task<OutboxEvent> ReadAndUpdateOutbox();
    public Task UpdateOutbox(ObjectId objectId);
}

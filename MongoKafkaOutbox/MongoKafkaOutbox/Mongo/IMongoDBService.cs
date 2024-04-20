using MongoDB.Bson;
using MongoDB.Driver;
using MongoKafkaOutbox.Outbox;

namespace MongoKafkaOutbox.Mongo;

public interface IMongoDBService
{
    public abstract IMongoCollection<BsonDocument> Collection { get; set; }
    public abstract IMongoCollection<OutboxEvent> OutboxCollection { get; set; }

    public Task AddToBothCollectionsWithTransaction(OutboxEvent outboxEvent, BsonDocument stuffDocument);
    public Task<OutboxEvent> ReadAndUpdateOutbox();
    public Task UpdateOutbox(ObjectId objectId);
}

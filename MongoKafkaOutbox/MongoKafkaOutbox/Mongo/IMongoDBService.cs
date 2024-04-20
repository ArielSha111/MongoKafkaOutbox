using MongoDB.Bson;
using MongoDB.Driver;
using MongoKafkaOutbox.Outbox;

namespace MongoKafkaOutbox.Mongo;

public interface IMongoDBService
{      
    IMongoCollection<OutboxEvent> OutboxCollection { get; }
  
    IMongoCollection<BsonDocument> StuffCollection { get; }

    public Task AddToBothCollectionsWithTransaction(OutboxEvent outboxEvent, BsonDocument stuffDocument);
    public Task<OutboxEvent> ReadAndUpdateOutbox();
}

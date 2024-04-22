using MongoDB.Bson;
using MongoDB.Driver;
using MongoKafkaOutbox.Outbox;

namespace MongoKafkaOutbox.Mongo;

public interface IMongoOutbox
{
    public abstract IMongoCollection<OutboxEvent> OutboxCollection { get; set; }
    public Task<bool> SaveChanges();

}

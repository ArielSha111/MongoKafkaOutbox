using MongoDB.Driver;
using MongoKafkaOutbox.Outbox;

namespace MongoKafkaOutbox.Mongo;

public abstract class MongoOutbox : IMongoOutbox
{
    public abstract IMongoCollection<OutboxEvent> OutboxCollection { get; set; }

    public async Task<bool> SaveChanges()
    {
        return true;
    }
}

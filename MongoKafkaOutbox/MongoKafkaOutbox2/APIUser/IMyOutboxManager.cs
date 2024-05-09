using MongoDB.Driver;
using MongoKafkaOutbox2.Outbox;

namespace MongoKafkaOutbox2.APIUser;

public interface IMyOutboxManager : IOutboxManager
{
    IMongoCollection<object> GeneralCollection { get; set; }
}
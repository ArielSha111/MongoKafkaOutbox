using MongoDB.Bson;

namespace MongoKafkaOutbox.Outbox;

public interface IOutboxService
{     
    Task Add(BsonDocument document);
    
    Task Publish<T>(T eventData, string topic);
   
    Task<bool> SaveChanges();
}

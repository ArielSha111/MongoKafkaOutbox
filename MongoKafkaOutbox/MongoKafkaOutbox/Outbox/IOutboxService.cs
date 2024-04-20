using MongoDB.Bson;

namespace MongoKafkaOutbox.Outbox;

public interface IOutboxService
{     
    Task Add(BsonDocument document);
    
    Task Publish<T>(T eventData);
   
    Task<bool> SaveChanges();
}

namespace MongoKafkaOutbox.Outbox;

public interface IOutboxService<T>
{     
    Task Add(T document);
    
    Task Publish<E>(E eventData, string topic);
   
    Task<bool> SaveChanges();
}

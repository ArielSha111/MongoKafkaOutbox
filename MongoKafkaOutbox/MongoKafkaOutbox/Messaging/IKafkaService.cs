using MongoKafkaOutbox.Outbox;

namespace MongoKafkaOutbox.Messaging;

public interface IKafkaService
{      
    Task ProduceMessageAsync(OutboxEvent outboxEvent);
}

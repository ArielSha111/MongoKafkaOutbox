using MongoDB.Bson;
using MongoKafkaOutbox.Messaging;
using MongoKafkaOutbox.Mongo;

namespace MongoKafkaOutbox.Outbox;

public class MyOutboxService : OutboxService
{
    public MyOutboxService()
    {
        _mongoDBService = new MongoDBService();
        _kafkaService = new KafkaService();
    }
}

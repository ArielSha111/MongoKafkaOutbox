using MongoDB.Bson;

namespace MongoKafkaOutbox.Outbox;

public class OutboxEvent
{
    public ObjectId Id { get; set; }
    public string Topic { get; set; }
    public object EventData { get; set; }
    public OutboxEventStatus eventStatus{ get; set; }
}
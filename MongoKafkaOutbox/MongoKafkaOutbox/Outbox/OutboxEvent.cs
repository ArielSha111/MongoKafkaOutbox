using MongoDB.Bson;

namespace MongoKafkaOutbox.Outbox;
public class OutboxEvent
{
    public ObjectId Id { get; set; }
    public string SessionId { get; set; }
    public string EventData { get; set; } // AVRO serialized data
    public bool Sent { get; set; }
}
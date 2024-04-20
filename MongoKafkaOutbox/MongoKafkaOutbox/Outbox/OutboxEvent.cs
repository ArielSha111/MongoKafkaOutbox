using MongoDB.Bson;

namespace MongoKafkaOutbox.Outbox;
public class OutboxEvent
{
    public ObjectId Id { get; set; } // Assuming ObjectId is a valid type in your code
    public string SessionId { get; set; }
    public object EventData { get; set; }
    public bool Sent { get; set; }
}
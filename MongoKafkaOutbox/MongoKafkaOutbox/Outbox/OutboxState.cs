using MongoDB.Bson;

namespace MongoKafkaOutbox.Outbox;

public class OutboxState
{
    public ObjectId Id { get; set; }
    public string SessionId { get; set; }
    public bool Completed { get; set; }
}
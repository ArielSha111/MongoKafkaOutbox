namespace MongoKafkaOutbox.Outbox;

public enum OutboxEventStatus
{
    Stored = 1,
    InProcess = 2,
    Published = 3
}

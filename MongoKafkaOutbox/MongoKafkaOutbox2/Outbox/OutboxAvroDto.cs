namespace MongoKafkaOutbox2.Outbox;

public class OutboxAvroDto
{
    internal Guid Id { get; set; }
    internal DateTime DateTime { get; set; }
    internal byte[] Payload { get; set; }
}

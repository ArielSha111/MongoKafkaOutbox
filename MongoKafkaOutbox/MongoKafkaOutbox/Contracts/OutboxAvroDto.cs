namespace MongoKafkaOutbox.Contracts;

public class OutboxAvroDto//todo adjust
{
    internal Guid Id { get; set; }
    internal DateTime DateTime { get; set; }
    internal byte[] Payload { get; set; }
}

namespace MongoKafkaOutbox2.Dtos;

public class OutboxDtoBase<T>
{
    internal Guid Id { get; set; }
    internal DateTime DateTime { get; set; }
    internal T Payload { get; set; }
}

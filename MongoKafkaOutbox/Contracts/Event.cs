using MongoKafkaOutbox.Records.Avro;

namespace Contracts;

public partial class Event : DynamicAvroRecord<Event>
{
    public string Description { get; set; } = "MessageSent";
    public string Id { get; set; } = Guid.NewGuid().ToString();
}

using MongoKafkaOutbox.Records;

namespace Contracts;

public class Event : SpecificAvroRecord<Event>
{
    public string Description { get; set; } = "MessageSent";
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public Person StoredPerson { get; set; }
}

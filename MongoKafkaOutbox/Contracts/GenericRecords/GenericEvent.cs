using MongoKafkaOutbox.Records;

namespace Contracts.SpecificRecords;

public class GenericEvent : GenericAvroRecord<GenericEvent>
{
    public string Description { get; set; } = "MessageSent";
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public GenericPerson StoredPerson { get; set; }
}
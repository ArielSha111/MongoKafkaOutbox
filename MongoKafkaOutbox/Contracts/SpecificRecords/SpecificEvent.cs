using MongoKafkaOutbox.Records.Specific;

namespace Contracts.SpecificRecords;

public class SpecificEvent : SpecificAvroRecord<SpecificEvent>
{
    public string Description { get; set; } = "MessageSent";
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public SpecificPerson StoredPerson { get; set; }
}

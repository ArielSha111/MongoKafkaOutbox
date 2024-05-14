using MongoKafkaOutbox.Records;

namespace Contracts.SpecificRecords;

public class GenericPerson : GenericAvroRecord<GenericEvent>
{
    public string Name { get; set; } = "AAAA";
    public int Age { get; set; } = 100;
}
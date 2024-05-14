using MongoKafkaOutbox.Records.Specific;

namespace Contracts.SpecificRecords;

public class SpecificPerson : SpecificAvroRecord<SpecificPerson>
{
    public string Name { get; set; } = "AAAA";
    public int Age { get; set; } = 100;
}

using MongoKafkaOutbox.Records;

namespace Contracts;

public class Person : SpecificAvroRecord<Person>
{
    public string Name { get; set; } = "AAAA";
    public int Age { get; set; } = 100;
}

using MongoKafkaOutbox2.Records.Avro;

namespace MongoKafkaOutbox2.APIUser;

public partial class Event : DynamicAvroRecord<Event>
{
    public string Description { get; set; } = "MessageSent";
    public DateTime TimeStamp { get; set; } = DateTime.Now;
}

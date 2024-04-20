using Confluent.Kafka;
using MongoKafkaOutbox.Outbox;

namespace MongoKafkaOutbox.Messaging;

public class KafkaService
{
    private readonly string _bootstrapServers;

    public KafkaService()
    {
        _bootstrapServers = "localhost:9092";       
    }

    public async Task ProduceMessageAsync(OutboxEvent outboxEvent)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = _bootstrapServers
        };
        var topic = "my_topic";

        using var producer = new ProducerBuilder<Null, string>(config).Build();

        var message = "outboxEvent.EventData";

        try
        {         
            var result = await producer.ProduceAsync(topic, new Message<Null, string> { Value = message });
            Console.WriteLine($"Produced message '{message}' to topic {result.Topic}, partition {result.Partition}, offset {result.Offset}");           
        }
        catch (ProduceException<Null, string> ex)
        {
            Console.WriteLine($"Delivery failed: {ex.Error.Reason}");
        }     
    }
}

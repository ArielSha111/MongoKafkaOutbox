using Confluent.Kafka;
using MongoKafkaOutbox.Outbox;

namespace MongoKafkaOutbox.Messaging;

public class KafkaService : IKafkaService
{
    private readonly string _bootstrapServers;
    private readonly string _topic;
    private readonly ProducerConfig ProducerConfig;

    public KafkaService()
    {
        _bootstrapServers = "localhost:9092";
        _topic = "my_topic";

        ProducerConfig = new ProducerConfig
        {
            BootstrapServers = _bootstrapServers
        };
    }

    public async Task ProduceMessageAsync(OutboxEvent outboxEvent)
    {
        var message = "outboxEvent.EventData";

        using var producer = new ProducerBuilder<Null, string>(ProducerConfig).Build();
        try
        {         
            var result = await producer.ProduceAsync(_topic, new Message<Null, string> { Value = message });
            Console.WriteLine($"Produced message '{message}' to topic {result.Topic}, partition {result.Partition}, offset {result.Offset}");           
        }
        catch (ProduceException<Null, string> ex)
        {
            Console.WriteLine($"Delivery failed: {ex.Error.Reason}");
        }     
    }
}

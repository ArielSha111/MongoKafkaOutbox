using Confluent.Kafka;
using MongoKafkaOutbox.Outbox;

namespace MongoKafkaOutbox.Messaging;

public abstract class KafkaService : IKafkaService
{
    protected ProducerConfig ProducerConfig { get; set; }

    public async Task ProduceMessageAsync(OutboxEvent outboxEvent)
    {
        var message = "outboxEvent.EventData";

        using var producer = new ProducerBuilder<Null, string>(ProducerConfig).Build();
        try
        {         
            var result = await producer.ProduceAsync(outboxEvent.Topic, new Message<Null, string> { Value = message });
            Console.WriteLine($"Produced message '{message}' to topic {result.Topic}, partition {result.Partition}, offset {result.Offset}");           
        }
        catch (ProduceException<Null, string> ex)
        {
            Console.WriteLine($"Delivery failed: {ex.Error.Reason}");
        }     
    }
}

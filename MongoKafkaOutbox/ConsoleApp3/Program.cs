using Confluent.Kafka;
using MongoDB.Bson.Serialization.Conventions;
using MongoDebeziumOutboxPattern;

ConfigMongo();

var cts = new CancellationTokenSource();

//var producer2 = Task.Run(() => StartProducer2(cts.Token));
var producer = Task.Run(() => StartProducer(cts.Token));
var consumer = Task.Run(() => StartConsumer(cts.Token));

Console.ReadKey();
cts.Cancel();
await Task.WhenAll(producer, consumer);

static async Task StartProducer(CancellationToken ct)
{
    var userService = new UserService();
    while (!ct.IsCancellationRequested)
    {
        await userService.CreateRandomUser();
        await Task.Delay(TimeSpan.FromSeconds(1));
    }
}

static async void StartProducer2(CancellationToken ct)
{
    var config = new ProducerConfig
    {
        BootstrapServers = "localhost:9092" // Change to your Kafka bootstrap servers

    };

    using (var producer = new ProducerBuilder<string, string>(config).Build())
    {
        try
        {
            var topic = "outbox.event.user"; // Change to the topic you want to produce to

            for (int i = 0; i < 10; i++)
            {
                var message = $"Message {i}";

                var deliveryReport = await producer.ProduceAsync(topic, new Message<string, string> { Value = message });

                Console.WriteLine($"Delivered message '{message}' to '{deliveryReport.TopicPartitionOffset}'");
            }
        }
        catch (ProduceException<Null, string> e)
        {
            Console.WriteLine($"Delivery failed: {e.Error.Reason}");
        }
    }
}

static void StartConsumer(CancellationToken ct)
{
    var config = new ConsumerConfig
    {
        BootstrapServers = "localhost:9092",
        GroupId = "test-group",
        AutoOffsetReset = AutoOffsetReset.Earliest,
        EnableAutoCommit = true
    };

    using (var consumer = new ConsumerBuilder<Ignore, string>(config).Build())
    {
        consumer.Subscribe("outbox.event.user");

        try
        {
            while (true)
            {
                var result = consumer.Consume();

                Console.WriteLine($"Received message: Key: {result.Message.Key}, Value: {result.Message.Value}");
            }
        }
        catch (OperationCanceledException)
        {
            consumer.Close();
        }
    }
}


void ConfigMongo()
{
    var conventionPack = new ConventionPack { new CamelCaseElementNameConvention() };
    ConventionRegistry.Register("camelCase", conventionPack, _ => true);
}
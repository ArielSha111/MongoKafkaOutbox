using ConsoleApp2;
using MongoDB.Bson;
using MongoKafkaOutbox.Outbox;

namespace YourConsoleAppNamespace
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var outboxService = new OutboxService<BsonDocument>(new MyMongoDBService(), new MyKafkaService());
            var stuffDocument = new BsonDocument
            {
                { "key", "value" },
                { "anotherKey", "anotherValue" }
                // Add more fields as needed
            };

            try
            {
                await outboxService.Add(stuffDocument);
                await outboxService.Publish(new { Name = "some name", Age = 1}, "my_topic");
                bool savedSuccessfully = await outboxService.SaveChanges();


                if (savedSuccessfully)
                {
                    Console.WriteLine("Changes saved successfully.");
                }
                else
                {
                    Console.WriteLine("Failed to save changes.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex}");
            }

            // Keep console window open
            Console.ReadLine();
        }
    }
}

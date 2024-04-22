using MongoDB.Bson;
using MongoKafkaOutbox.Mongo;

namespace ConsoleApp2
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var mongoOutboxClient = new MyMongoClient();
            var KafkaOutboxClient = new MyKafkaProducer();

            var stuffDocument = new BsonDocument
            {
                { "key", "value" },
                { "anotherKey", "anotherValue" }
                // Add more fields as needed
            };

            try
            {
                await mongoOutboxClient.AddStuff(stuffDocument);
                await KafkaOutboxClient.Publish(new { Name = "some name", Age = 1 }, "my_topic");
                bool savedSuccessfully = await mongoOutboxClient.SaveChanges();


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

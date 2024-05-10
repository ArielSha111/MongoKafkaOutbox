using Service;
using Model.DB;
using Contracts;
using MongoKafkaOutbox.DI;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoKafkaOutbox.Outbox.Default;

public static class Startup
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration Configuration)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddSingleton(Configuration);
        SetDiRegistration(services);
    }

    private static void SetDiRegistration(IServiceCollection services)
    {
        services.AddSingleton<IExampleService, ExampleService>();
        services.AddSingleton<IDbManagerWithOutBox, DbManagerWithOutbox>();
        services.SetOutboxServicesWithDefaults<Person>(new()
        {
            MongoConnectionString = "mongodb://localhost:28017",
            MongoDBName = "KafkaOutbox",
            MongoCollectionNames = new Dictionary<string, string>() { { "MainCollection", "MainCollection" } },
            OutboxCollectionName = "Outbox",
            schemaRegistryConfigUrl = "http://localhost:8081",
            kafkaTopicName = "MongoKafkaOutboxTopic1",
        }); 
    }
}


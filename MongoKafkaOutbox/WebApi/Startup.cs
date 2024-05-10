using Service;
using Model.DB;
using Contracts;
using MongoKafkaOutbox.DI;

public static class Startup
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddSingleton(configuration);
        SetDiRegistration(services, configuration);
    }

    private static void SetDiRegistration(IServiceCollection services, IConfiguration configuration)
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


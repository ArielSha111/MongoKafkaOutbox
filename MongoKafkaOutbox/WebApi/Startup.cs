using Service;
using Model.DB;
using Contracts;
using MongoKafkaOutbox.DI;
using MongoDB.Driver;

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

        services.AddSingleton<IMongoClient>(sp =>
        {
            var client = new MongoClient("mongodb://localhost:28017");
            return client;
        });

        services.SetOutboxServicesWithDefaults(new()
        {
            OutboxDbName = "KafkaOutbox",
            OutboxCollectionName = "Outbox",
            schemaRegistryConfigUrl = "http://localhost:8081",
            kafkaTopicName = "MongoKafkaOutboxTopic1",
        }); 
    }
}


using MongoKafkaOutbox2.Outbox.Default;

namespace MongoKafkaOutbox2.APIUser;

internal class TestClass(IGenericOutboxManager<Person> genericOutboxManager)
{
    public async Task SomeMethod()
    {
        using var session = await genericOutboxManager.StartOutboxSessionAsync();
        session.StartTransaction();
        try
        {
            await genericOutboxManager.Collection.InsertOneAsync(new Person());
            await genericOutboxManager.PublishMessage(new Event());
            await session.CommitTransactionAsync();
        }
        catch (Exception)
        {
            await session.AbortTransactionAsync();
        }
    }
}

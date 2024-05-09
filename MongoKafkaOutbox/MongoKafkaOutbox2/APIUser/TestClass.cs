namespace MongoKafkaOutbox2.APIUser;

internal class TestClass(IMyOutboxManager baseOutboxManager)
{
    public async Task SomeMethod()
    {
        using var session = await baseOutboxManager.StartSessionAsync();
        session.StartTransaction();

        try
        {
            await baseOutboxManager.GeneralCollection.InsertOneAsync(new());
            await session.CommitTransactionAsync();
        }

        catch (Exception ex)
        {
            await session.AbortTransactionAsync();
        }
    }
}

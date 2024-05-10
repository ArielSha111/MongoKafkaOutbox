using Contracts;
using MongoKafkaOutbox.Outbox.Default;

namespace Model.DB;

public class DbManagerWithOutbox(IGenericOutboxManager<Person> genericOutboxManager) : IDbManagerWithOutBox
{
    public async Task PutStuffInDbWithOutbox()
    {
        using var session = await genericOutboxManager.StartOutboxSessionAsync();
        session.StartTransaction();
        try
        {
            await genericOutboxManager.Collection.InsertOneAsync(new Person());
            await genericOutboxManager.PublishMessageWithOutbox(new Event());
            await session.CommitTransactionAsync();
        }
        catch (Exception)
        {
            await session.AbortTransactionAsync();
        }
    }
}
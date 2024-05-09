using MongoDB.Driver;

namespace MongoKafkaOutbox2.Outbox;

public class OutboxClientSessionHandle(IClientSessionHandle clientSessionHandle) : IOutboxClientSessionHandle, IDisposable 
{
    public void StartTransaction(TransactionOptions transactionOptions = null)
    {
        clientSessionHandle.StartTransaction(transactionOptions);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {

        await clientSessionHandle.CommitTransactionAsync(cancellationToken);
    }

    public async Task AbortTransactionAsync(CancellationToken cancellationToken = default)
    {
        await clientSessionHandle.AbortTransactionAsync(cancellationToken);
    }

    public void Dispose()
    {
        clientSessionHandle.Dispose();
    }
}

using MongoDB.Driver;

namespace MongoKafkaOutbox2.MongoSessionHandlers;

public interface IOutboxClientSessionHandle : IDisposable
{
    public void StartTransaction(TransactionOptions transactionOptions = null);
    public Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    public Task AbortTransactionAsync(CancellationToken cancellationToken = default);
    public void Dispose();
}
using MongoDB.Driver;

namespace MongoKafkaOutbox2.Outbox
{
    public interface IOutboxClientSessionHandle
    {
        Task AbortTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        void Dispose();
        void StartTransaction(TransactionOptions transactionOptions = null);
    }
}
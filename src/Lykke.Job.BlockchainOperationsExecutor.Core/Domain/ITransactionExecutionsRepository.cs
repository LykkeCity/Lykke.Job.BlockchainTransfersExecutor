using System;
using System.Threading.Tasks;

namespace Lykke.Job.BlockchainOperationsExecutor.Core.Domain
{
    public interface ITransactionExecutionsRepository
    {
        Task<TransactionExecutionAggregate> GetOrAddAsync(Guid operationId, Func<TransactionExecutionAggregate> newAggregateFactory);
        Task<TransactionExecutionAggregate> GetAsync(Guid operationId);
        Task SaveAsync(TransactionExecutionAggregate aggregate);
        Task<TransactionExecutionAggregate> TryGetAsync(Guid operationId);
    }
}

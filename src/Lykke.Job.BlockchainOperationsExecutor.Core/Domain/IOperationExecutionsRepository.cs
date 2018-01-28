using System;
using System.Threading.Tasks;

namespace Lykke.Job.BlockchainOperationsExecutor.Core.Domain
{
    public interface IOperationExecutionsRepository
    {
        Task<OperationExecutionAggregate> GetOrAddAsync(Guid operationId, Func<OperationExecutionAggregate> newAggregateFactory);
        Task<OperationExecutionAggregate> GetAsync(Guid operationId);
        Task SaveAsync(OperationExecutionAggregate aggregate);
        Task<OperationExecutionAggregate> TryGetAsync(Guid operationId);
    }
}

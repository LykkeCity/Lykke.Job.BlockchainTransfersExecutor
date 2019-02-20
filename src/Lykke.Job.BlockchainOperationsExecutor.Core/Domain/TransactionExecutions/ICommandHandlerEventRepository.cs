using System;
using System.Threading.Tasks;

namespace Lykke.Job.BlockchainOperationsExecutor.Core.Domain.TransactionExecutions
{
    public interface ICommandHandlerEventRepository
    {
        Task<object> TryGetEventAsync(Guid aggregateId, string commandHandlerId);
        Task<T> InsertEventAsync<T>(Guid aggregateId, string commandHandlerId, T eventData);
    }
}

using System;
using System.Threading.Tasks;

namespace Lykke.Job.BlockchainOperationsExecutor.Core.Domain.TransactionExecutions
{
    public interface IAddressLocksRepository
    {
        Task ConcurrentlyLockInputAsync(string blockchainType, string address, Guid operationId);

        Task ConcurrentlyLockOutputAsync(string blockchainType, string address, Guid operationId);
        
        Task<bool> IsInputInExclusiveLockAsync(string blockchainType, string address);

        Task ReleaseInputExclusiveLockAsync(string blockchainType, string address);
        
        Task ReleaseOutputExclusiveLockAsync(string blockchainType, string address);

        Task ReleaseInputConcurrentLockAsync(string blockchainType, string address, Guid operationId);
        
        Task ReleaseOutputConcurrentLockAsync(string blockchainType, string address, Guid operationId);

        Task<bool> TryExclusivelyLockInputAsync(string blockchainType, string address, Guid operationId);
        
        Task<bool> TryExclusivelyLockOutputAsync(string blockchainType, string address, Guid operationId);
    }
}

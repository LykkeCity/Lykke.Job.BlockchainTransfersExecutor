using System;
using System.Threading.Tasks;

namespace Lykke.Job.BlockchainOperationsExecutor.Core.Domain.TransactionExecutions
{
    public interface IAddressLocksRepository
    {
        Task ConcurrentlyLockInputAsync(string blockchainType, string address, Guid transactionId);

        Task ConcurrentlyLockOutputAsync(string blockchainType, string address, Guid transactionId);
        
        Task<bool> IsInputInExclusiveLockAsync(string blockchainType, string address);

        Task ReleaseInputExclusiveLockAsync(string blockchainType, string address, Guid transactionId);
        
        Task ReleaseOutputExclusiveLockAsync(string blockchainType, string address, Guid transactionId);

        Task ReleaseInputConcurrentLockAsync(string blockchainType, string address, Guid transactionId);
        
        Task ReleaseOutputConcurrentLockAsync(string blockchainType, string address, Guid transactionId);

        Task<bool> TryExclusivelyLockInputAsync(string blockchainType, string address, Guid transactionId);
        
        Task<bool> TryExclusivelyLockOutputAsync(string blockchainType, string address, Guid transactionId);
    }
}

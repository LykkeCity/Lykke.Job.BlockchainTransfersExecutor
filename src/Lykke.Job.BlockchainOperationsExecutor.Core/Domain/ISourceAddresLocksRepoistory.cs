using System;
using System.Threading.Tasks;

namespace Lykke.Job.BlockchainOperationsExecutor.Core.Domain
{
    public interface ISourceAddresLocksRepoistory
    {
        Task<bool> TryGetLockAsync(string blockchainType, string address, Guid transactionId);
        Task ReleaseLockAsync(string blockchainType, string address, Guid transactionId);
    }
}

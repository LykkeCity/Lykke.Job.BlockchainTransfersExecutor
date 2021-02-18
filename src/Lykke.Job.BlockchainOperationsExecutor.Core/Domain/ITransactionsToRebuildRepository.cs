using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Job.BlockchainOperationsExecutor.Core.Domain
{
    public interface ITransactionsToRebuildRepository
    {
        Task<IReadOnlyCollection<Guid>> GetAll();
        Task AddOrReplace(Guid operationId);
        Task EnsureRemoved(Guid operationId);
        Task<bool> Contains(Guid operationId);
    }
}

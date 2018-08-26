using System;
using System.Threading.Tasks;

namespace Lykke.Job.BlockchainOperationsExecutor.Core.Domain.OperationExecutions
{
    public interface IActiveTransactionsRepository
    {
        Task<Guid> GetOrStartTransactionAsync(Guid operationId, Func<Guid> newTransactionIdFactory);
        Task EndTransactionAsync(Guid operationId, Guid transactionId);
    }
}
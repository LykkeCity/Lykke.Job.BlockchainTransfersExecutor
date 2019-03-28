using System;
using Lykke.AzureStorage.Tables;

namespace Lykke.Job.BlockchainOperationsExecutor.AzureRepositories.TransactionExecutions
{
    public class AddressLockEntity : AzureTableEntity
    {
        public Guid LockOwner { get; set; }
    }
}

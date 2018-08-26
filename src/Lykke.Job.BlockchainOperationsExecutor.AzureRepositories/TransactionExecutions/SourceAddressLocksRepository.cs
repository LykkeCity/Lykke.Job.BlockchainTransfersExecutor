using System;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.TransactionExecutions;
using Lykke.SettingsReader;

namespace Lykke.Job.BlockchainOperationsExecutor.AzureRepositories.TransactionExecutions
{
    [UsedImplicitly]
    public class SourceAddressLocksRepository : ISourceAddresLocksRepoistory
    {
        private readonly INoSQLTableStorage<SourceAddressLockEntity> _storage;

        public static ISourceAddresLocksRepoistory Create(IReloadingManager<string> connectionString, ILog log)
        {
            var storage = AzureTableStorage<SourceAddressLockEntity>.Create(
                connectionString,
                "SourceAddressLocks",
                log);

            return new SourceAddressLocksRepository(storage);
        }

        private SourceAddressLocksRepository(INoSQLTableStorage<SourceAddressLockEntity> storage)
        {
            _storage = storage;
        }

        public async Task<bool> TryGetLockAsync(string blockchainType, string address, Guid transactionId)
        {
            var partitionKey = SourceAddressLockEntity.GetPartitionKey(blockchainType, address);
            var rowKey = SourceAddressLockEntity.GetRowKey(address);

            var lockEntity = await _storage.GetOrInsertAsync(partitionKey, rowKey,
                () => new SourceAddressLockEntity
                {
                    PartitionKey = partitionKey,
                    RowKey = rowKey,
                    OwnerTransactionId = transactionId
                });

            return lockEntity.OwnerTransactionId == transactionId;
        }

        public async Task ReleaseLockAsync(string blockchainType, string address, Guid transactionId)
        {
            var partitionKey = SourceAddressLockEntity.GetPartitionKey(blockchainType, address);
            var rowKey = SourceAddressLockEntity.GetRowKey(address);
            
            await _storage.DeleteIfExistAsync(
                partitionKey, 
                rowKey,
                // Exactly the given transaction should own current lock to remove it
                lockEntity => lockEntity.OwnerTransactionId == transactionId);
        }
    }
}

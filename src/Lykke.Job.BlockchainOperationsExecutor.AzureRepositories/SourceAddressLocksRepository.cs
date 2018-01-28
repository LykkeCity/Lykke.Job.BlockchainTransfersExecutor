using System;
using System.Net;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain;
using Lykke.SettingsReader;
using Microsoft.WindowsAzure.Storage;

namespace Lykke.Job.BlockchainOperationsExecutor.AzureRepositories
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

        public async Task<bool> TryGetLockAsync(string blockchainType, string address, Guid operationId)
        {
            var partitionKey = SourceAddressLockEntity.GetPartitionKey(blockchainType, address);
            var rowKey = SourceAddressLockEntity.GetRowKey(address);

            var lockEntity = await _storage.GetOrInsertAsync(partitionKey, rowKey,
                () => new SourceAddressLockEntity
                {
                    PartitionKey = partitionKey,
                    RowKey = rowKey,
                    OwnerOperationId = operationId
                });

            return lockEntity.OwnerOperationId == operationId;
        }

        public async Task ReleaseLockAsync(string blockchainType, string address, Guid operationId)
        {
            var partitionKey = SourceAddressLockEntity.GetPartitionKey(blockchainType, address);
            var rowKey = SourceAddressLockEntity.GetRowKey(address);

            var lockEntity = await _storage.GetDataAsync(partitionKey, rowKey);

            if (lockEntity != null)
            {
                // Exactly the given operation should own current lock to remove it

                if (lockEntity.OwnerOperationId == operationId)
                {
                    try
                    {
                        await _storage.DeleteAsync(lockEntity);
                    }
                    catch (StorageException e) when (e.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
                    {
                        // Lock has been already removed, so just ignores this exception
                    }
                }
            }
        }
    }
}

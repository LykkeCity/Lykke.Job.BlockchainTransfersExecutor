using System;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.TransactionExecutions;
using Lykke.SettingsReader;

namespace Lykke.Job.BlockchainOperationsExecutor.AzureRepositories.TransactionExecutions
{
    [UsedImplicitly]
    public class AddressLocksRepository : IAddressLocksRepository
    {
        private const string Input = "input";
        private const string Output = "output";
        
        private readonly INoSQLTableStorage<AddressLockEntity> _storage;

        public static IAddressLocksRepository Create(IReloadingManager<string> connectionString, ILogFactory logFactory)
        {
            var storage = AzureTableStorage<AddressLockEntity>.Create(
                connectionString,
                "AddressLocks",
                logFactory);

            return new AddressLocksRepository(storage);
        }

        private AddressLocksRepository(INoSQLTableStorage<AddressLockEntity> storage)
        {
            _storage = storage;
        }

        #region Interface implementation
        
        public Task ConcurrentlyLockInputAsync(string blockchainType, string address, Guid operationId)
            => ConcurrentlyLockAddressAsync(blockchainType, address, operationId, Input);

        public Task ConcurrentlyLockOutputAsync(string blockchainType, string address, Guid operationId)
            => ConcurrentlyLockAddressAsync(blockchainType, address, operationId, Output);
        
        public Task<bool> IsInputInExclusiveLockAsync(string blockchainType, string address)
            => IsAddressInExclusiveLockAsync(blockchainType, address, Input);

        public Task ReleaseInputConcurrentLockAsync(string blockchainType, string address, Guid operationId)
            => ReleaseAddressConcurrentLockAsync(blockchainType, address, operationId,Input);

        public Task ReleaseInputExclusiveLockAsync(string blockchainType, string address)
            => ReleaseAddressExclusiveLockAsync(blockchainType, address, Input);

        public Task ReleaseOutputConcurrentLockAsync(string blockchainType, string address, Guid operationId)
            => ReleaseAddressConcurrentLockAsync(blockchainType, address, operationId,Output);

        public Task ReleaseOutputExclusiveLockAsync(string blockchainType, string address)
            => ReleaseAddressExclusiveLockAsync(blockchainType, address, Output);

        public Task<bool> TryExclusivelyLockInputAsync(string blockchainType, string address, Guid operationId)
            => TryExclusivelyLockAddressAsync(blockchainType, address, operationId, Input);

        public Task<bool> TryExclusivelyLockOutputAsync(string blockchainType, string address, Guid operationId)
            => TryExclusivelyLockAddressAsync(blockchainType, address, operationId, Output);

        #endregion

        private Task ConcurrentlyLockAddressAsync(
            string blockchainType,
            string address,
            Guid operationId,
            string direction)
        {
            var partitionKey = GetPartitionKey(blockchainType, address, direction);
            var rowKey = GetConcurrentLockRowKey(operationId);
            
            return _storage.CreateIfNotExistsAsync(new AddressLockEntity
            {
                PartitionKey = partitionKey,
                RowKey = rowKey
            });
        }

        private async Task<bool> IsAddressInConcurrentLockAsync(
            string blockchainType,
            string address,
            string direction)
        {
            var partitionKey = GetPartitionKey(blockchainType, address, direction);

            return (await _storage.GetDataAsync(partitionKey))
                .Any(x => x.RowKey != GetExclusiveLockRowKey());
        }

        private async Task<bool> IsAddressInExclusiveLockAsync(
            string blockchainType,
            string address,
            string direction)
        {
            var partitionKey = GetPartitionKey(blockchainType, address, direction);

            return (await _storage.GetDataAsync(partitionKey))
                .Any(x => x.RowKey == GetExclusiveLockRowKey());
        }

        private Task ReleaseAddressConcurrentLockAsync(
            string blockchainType,
            string address,
            Guid operationId,
            string direction)
        {
            var partitionKey = GetPartitionKey(blockchainType, address, direction);
            var rowKey = GetConcurrentLockRowKey(operationId);
            
            return _storage.DeleteIfExistAsync(partitionKey, rowKey);
        }

        private Task ReleaseAddressExclusiveLockAsync(
            string blockchainType,
            string address,
            string direction)
        {
            var partitionKey = GetPartitionKey(blockchainType, address, direction);
            var rowKey = GetExclusiveLockRowKey();
            
            return _storage.DeleteIfExistAsync(partitionKey, rowKey);
        }

        private async Task<bool> TryExclusivelyLockAddressAsync(
            string blockchainType,
            string address,
            Guid operationId,
            string direction)
        {
            if (await IsAddressInConcurrentLockAsync(blockchainType, address, direction))
            {
                return false;
            }
            else
            {
                var partitionKey = GetPartitionKey(blockchainType, address, direction);
                var rowKey = GetExclusiveLockRowKey();
                
                AddressLockEntity LockFactory()
                {
                    return new AddressLockEntity
                    {
                        PartitionKey = partitionKey,
                        RowKey = rowKey,
                        LockOwner = operationId
                    };
                }
                
                var @lock = await _storage.GetOrInsertAsync
                (
                    partitionKey,
                    rowKey,
                    LockFactory
                );

                return @lock.LockOwner == operationId;
            }
        }

        #region Entity Keys
        
        private static string GetPartitionKey(string blockchainType, string address, string direction)
            => $"{blockchainType}-{address}-{direction}";

        private static string GetExclusiveLockRowKey()
            => "exclusive";

        private static string GetConcurrentLockRowKey(Guid operationId)
            => $"concurrent-{operationId}";
        
        #endregion
    }
}

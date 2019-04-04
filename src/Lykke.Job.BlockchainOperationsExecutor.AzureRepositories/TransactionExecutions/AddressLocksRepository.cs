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
        
        public Task ConcurrentlyLockInputAsync(string blockchainType, string address, Guid transactionId)
            => ConcurrentlyLockAddressAsync(blockchainType, address, transactionId, Input);

        public Task ConcurrentlyLockOutputAsync(string blockchainType, string address, Guid transactionId)
            => ConcurrentlyLockAddressAsync(blockchainType, address, transactionId, Output);
        
        public Task<bool> IsInputInExclusiveLockAsync(string blockchainType, string address)
            => IsAddressInExclusiveLockAsync(blockchainType, address, Input);

        public Task ReleaseInputConcurrentLockAsync(string blockchainType, string address, Guid transactionId)
            => ReleaseAddressConcurrentLockAsync(blockchainType, address, transactionId,Input);

        public Task ReleaseInputExclusiveLockAsync(string blockchainType, string address, Guid transactionId)
            => ReleaseAddressExclusiveLockAsync(blockchainType, address, transactionId, Input);

        public Task ReleaseOutputConcurrentLockAsync(string blockchainType, string address, Guid transactionId)
            => ReleaseAddressConcurrentLockAsync(blockchainType, address, transactionId,Output);

        public Task ReleaseOutputExclusiveLockAsync(string blockchainType, string address, Guid transactionId)
            => ReleaseAddressExclusiveLockAsync(blockchainType, address, transactionId, Output);

        public Task<bool> TryExclusivelyLockInputAsync(string blockchainType, string address, Guid transactionId)
            => TryExclusivelyLockAddressAsync(blockchainType, address, transactionId, Input);

        public Task<bool> TryExclusivelyLockOutputAsync(string blockchainType, string address, Guid transactionId)
            => TryExclusivelyLockAddressAsync(blockchainType, address, transactionId, Output);

        #endregion

        private Task ConcurrentlyLockAddressAsync(
            string blockchainType,
            string address,
            Guid transactionId,
            string direction)
        {
            var partitionKey = GetPartitionKey(blockchainType, address, direction);
            var rowKey = GetConcurrentLockRowKey(transactionId);
            
            return _storage.CreateIfNotExistsAsync(new AddressLockEntity
            {
                PartitionKey = partitionKey,
                RowKey = rowKey,
                LockOwner = transactionId
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
            var rowKey = GetExclusiveLockRowKey();

            return await _storage.GetDataAsync(partitionKey, rowKey) != null;
        }

        private Task ReleaseAddressConcurrentLockAsync(
            string blockchainType,
            string address,
            Guid transactionId,
            string direction)
        {
            var partitionKey = GetPartitionKey(blockchainType, address, direction);
            var rowKey = GetConcurrentLockRowKey(transactionId);
            
            return _storage.DeleteIfExistAsync(partitionKey, rowKey);
        }

        private Task ReleaseAddressExclusiveLockAsync(
            string blockchainType,
            string address,
            Guid transactionId,
            string direction)
        {
            var partitionKey = GetPartitionKey(blockchainType, address, direction);
            var rowKey = GetExclusiveLockRowKey();
            
            return _storage.DeleteIfExistAsync(partitionKey, rowKey, @lock => @lock.LockOwner == transactionId);
        }

        private async Task<bool> TryExclusivelyLockAddressAsync(
            string blockchainType,
            string address,
            Guid transactionId,
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
                        LockOwner = transactionId
                    };
                }
                
                var @lock = await _storage.GetOrInsertAsync
                (
                    partitionKey,
                    rowKey,
                    LockFactory
                );

                return @lock.LockOwner == transactionId;
            }
        }

        #region Entity Keys
        
        private static string GetPartitionKey(string blockchainType, string address, string direction)
            => $"{blockchainType}-{address}-{direction}";

        private static string GetExclusiveLockRowKey()
            => "exclusive";

        private static string GetConcurrentLockRowKey(Guid transactionId)
            => $"concurrent-{transactionId}";
        
        #endregion
    }
}

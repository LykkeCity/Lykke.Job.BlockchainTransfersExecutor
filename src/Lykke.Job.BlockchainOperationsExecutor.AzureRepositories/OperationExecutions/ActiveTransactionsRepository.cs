using System;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.OperationExecutions;
using Lykke.SettingsReader;

namespace Lykke.Job.BlockchainOperationsExecutor.AzureRepositories.OperationExecutions
{
    [UsedImplicitly]
    public class ActiveTransactionsRepository : IActiveTransactionsRepository
    {
        private readonly INoSQLTableStorage<ActiveTransactionEntity> _storage;

        public static IActiveTransactionsRepository Create(IReloadingManager<string> connectionString, ILogFactory logFactory)
        {
            var storage = AzureTableStorage<ActiveTransactionEntity>.Create(
                connectionString,
                "ActiveTransactions",
                logFactory);

            return new ActiveTransactionsRepository(storage);
        }

        private ActiveTransactionsRepository(INoSQLTableStorage<ActiveTransactionEntity> storage)
        {
            _storage = storage;
        }

        public async Task<Guid> GetOrStartTransactionAsync(Guid operationId, Func<Guid> newTransactionIdFactory)
        {
            var partitionKey = ActiveTransactionEntity.GetPartitionKey(operationId);
            var rowKey = ActiveTransactionEntity.GetRowKey(operationId);

            var entity = await _storage.GetOrInsertAsync(
                partitionKey,
                rowKey,
                () => new ActiveTransactionEntity
                {
                    PartitionKey = partitionKey,
                    RowKey = rowKey,
                    TransactionId = newTransactionIdFactory()
                });

            return entity.TransactionId;
        }

        public Task EndTransactionAsync(Guid operationId, Guid transactionId)
        {
            var partitionKey = ActiveTransactionEntity.GetPartitionKey(operationId);
            var rowKey = ActiveTransactionEntity.GetRowKey(operationId);

            return _storage.DeleteIfExistAsync(
                partitionKey, 
                rowKey, 
                entity => entity.TransactionId == transactionId);
        }
    }
}

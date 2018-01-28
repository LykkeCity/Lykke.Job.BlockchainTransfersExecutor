using System;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain;
using Lykke.SettingsReader;

namespace Lykke.Job.BlockchainOperationsExecutor.AzureRepositories
{
    [UsedImplicitly]
    public class OperationExecutionsRepository : IOperationExecutionsRepository
    {
        private readonly INoSQLTableStorage<OperationExecutionEntity> _storage;

        public static IOperationExecutionsRepository Create(IReloadingManager<string> connectionString, ILog log)
        {
            var storage = AzureTableStorage<OperationExecutionEntity>.Create(
                connectionString,
                "OperationExecutions",
                log);

            return new OperationExecutionsRepository(storage);
        }

        private OperationExecutionsRepository(INoSQLTableStorage<OperationExecutionEntity> storage)
        {
            _storage = storage;
        }

        public async Task<OperationExecutionAggregate> GetOrAddAsync(Guid operationId, Func<OperationExecutionAggregate> newAggregateFactory)
        {
            var partitionKey = OperationExecutionEntity.GetPartitionKey(operationId);
            var rowKey = OperationExecutionEntity.GetRowKey(operationId);

            var startedEntity = await _storage.GetOrInsertAsync(
                partitionKey,
                rowKey,
                () =>
                {
                    var newAggregate = newAggregateFactory();

                    return OperationExecutionEntity.FromDomain(newAggregate);
                });

            return startedEntity.ToDomain();
        }

        public async Task<OperationExecutionAggregate> GetAsync(Guid operationId)
        {
            var aggregate = await TryGetAsync(operationId);

            if (aggregate == null)
            {
                throw new InvalidOperationException($"Operation execution with operation ID [{operationId}] is not found");
            }

            return aggregate;
        }

        public async Task<OperationExecutionAggregate> TryGetAsync(Guid operationId)
        {
            var partitionKey = OperationExecutionEntity.GetPartitionKey(operationId);
            var rowKey = OperationExecutionEntity.GetRowKey(operationId);

            var entity = await _storage.GetDataAsync(partitionKey, rowKey);

            return entity?.ToDomain();
        }

        public async Task SaveAsync(OperationExecutionAggregate aggregate)
        {
            var entity = OperationExecutionEntity.FromDomain(aggregate);

            await _storage.ReplaceAsync(entity);
        }
    }
}

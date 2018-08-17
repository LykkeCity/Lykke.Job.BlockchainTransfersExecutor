using System;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.AzureStorage.Tables;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain;

namespace Lykke.Job.BlockchainOperationsExecutor.AzureRepositories
{
    internal class AggregateRepository<TAggregate, TEntity> : IAggregateRepository<TAggregate>
        where TAggregate : class
        where TEntity : AzureTableEntity, new()
    {
        private readonly INoSQLTableStorage<TEntity> _storage;
        private readonly Func<TAggregate, TEntity> _mapAggregateToEntity;
        private readonly Func<TEntity, Task<TAggregate>> _mapEntityToAggregate;

        public AggregateRepository(
            INoSQLTableStorage<TEntity> storage,
            Func<TAggregate, TEntity> mapAggregateToEntity,
            Func<TEntity, Task<TAggregate>> mapEntityToAggregate)
        {
            _storage = storage;
            _mapAggregateToEntity = mapAggregateToEntity;
            _mapEntityToAggregate = mapEntityToAggregate;
        }

        public async Task<TAggregate> GetOrAddAsync(Guid aggregateId, Func<TAggregate> newAggregateFactory)
        {
            var partitionKey = AggregateKeysBuilder.BuildPartitionKey(aggregateId);
            var rowKey = AggregateKeysBuilder.BuildRowKey(aggregateId);

            var startedEntity = await _storage.GetOrInsertAsync(
                partitionKey,
                rowKey,
                () =>
                {
                    var newAggregate = newAggregateFactory();

                    return _mapAggregateToEntity(newAggregate);
                });
            
            return await _mapEntityToAggregate(startedEntity);
        }

        public async Task<TAggregate> GetAsync(Guid operationId)
        {
            var aggregate = await TryGetAsync(operationId);

            if (aggregate == null)
            {
                throw new InvalidOperationException($"Aggregate [{typeof(TAggregate).Name}] with ID [{operationId}] is not found");
            }

            return aggregate;
        }

        public async Task<TAggregate> TryGetAsync(Guid aggregateId)
        {
            var partitionKey = AggregateKeysBuilder.BuildPartitionKey(aggregateId);
            var rowKey = AggregateKeysBuilder.BuildRowKey(aggregateId);

            var entity = await _storage.GetDataAsync(partitionKey, rowKey);
            
            if (entity != null)
            {
                return await _mapEntityToAggregate(entity);
            }

            return null;
        }

        public Task SaveAsync(TAggregate aggregate)
        {
            var entity = _mapAggregateToEntity(aggregate);

            return _storage.ReplaceAsync(entity);
        }
    }
}

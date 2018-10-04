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
    public class OperationExecutionsRepository : IOperationExecutionsRepository
    {
        private readonly AggregateRepository<OperationExecutionAggregate, OperationExecutionEntity> _aggregateRepository;
        
        public static IOperationExecutionsRepository Create(IReloadingManager<string> connectionString, ILogFactory logFactory)
        {
            var storage = AzureTableStorage<OperationExecutionEntity>.Create(
                connectionString,
                "OperationExecutions",
                logFactory);

            return new OperationExecutionsRepository(storage);
        }

        private OperationExecutionsRepository(INoSQLTableStorage<OperationExecutionEntity> storage)
        {
            _aggregateRepository = new AggregateRepository<OperationExecutionAggregate, OperationExecutionEntity>(
                storage,
                mapAggregateToEntity: OperationExecutionEntity.FromDomain,
                mapEntityToAggregate: entity => Task.FromResult(entity.ToDomain()));
        }

        public Task<OperationExecutionAggregate> GetOrAddAsync(Guid operationId, Func<OperationExecutionAggregate> newAggregateFactory)
        {
            return _aggregateRepository.GetOrAddAsync(operationId, newAggregateFactory);
        }

        public Task<OperationExecutionAggregate> GetAsync(Guid operationId)
        {
            return _aggregateRepository.GetAsync(operationId);
        }

        public Task<OperationExecutionAggregate> TryGetAsync(Guid operationId)
        {
            return _aggregateRepository.TryGetAsync(operationId);
        }

        public Task SaveAsync(OperationExecutionAggregate aggregate)
        {
            return _aggregateRepository.SaveAsync(aggregate);
        }
    }
}

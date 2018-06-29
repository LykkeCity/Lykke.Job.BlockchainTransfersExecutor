using System;
using System.IO;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Blob;
using AzureStorage.Tables;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain;
using Lykke.SettingsReader;
using Newtonsoft.Json;

namespace Lykke.Job.BlockchainOperationsExecutor.AzureRepositories
{
    [UsedImplicitly]
    public class OperationExecutionsRepository : IOperationExecutionsRepository
    {
        private readonly INoSQLTableStorage<OperationExecutionEntity> _storage;
        private readonly IBlobStorage _blob;
        private readonly JsonSerializer _jsonSerializer;

        public static IOperationExecutionsRepository Create(IReloadingManager<string> connectionString, ILog log)
        {
            var storage = AzureTableStorage<OperationExecutionEntity>.Create(
                connectionString,
                "OperationExecutions",
                log);
            var blob = AzureBlobStorage.Create(connectionString);

            return new OperationExecutionsRepository(storage, blob);
        }

        private OperationExecutionsRepository(INoSQLTableStorage<OperationExecutionEntity> storage, IBlobStorage blob)
        {
            _storage = storage;
            _blob = blob;

            _jsonSerializer = new JsonSerializer();
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

            var blobEntity = await TryGetBlobEntityAsync(operationId, startedEntity.BlockchainType);
            
            return startedEntity.ToDomain(blobEntity);
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
            
            if (entity != null)
            {
                var blobEntity = await TryGetBlobEntityAsync(operationId, entity.BlockchainType);

                return entity.ToDomain(blobEntity);
            }

            return null;
        }

        public Task SaveAsync(OperationExecutionAggregate aggregate)
        {
            var entity = OperationExecutionEntity.FromDomain(aggregate);
            var blobEntity = OperationExecutionBlobEntity.FromDomain(aggregate);

            return Task.WhenAll
            (
                SaveBlobEntityAsync(aggregate.OperationId, aggregate.BlockchainType, blobEntity),
                _storage.ReplaceAsync(entity)
            );
        }

        private async Task<OperationExecutionBlobEntity> TryGetBlobEntityAsync(
            Guid operationId,
            string blockchainType)
        {
            // TODO: Could be removed, when obsolete field TransactionBuiltEvent.BlockchainType will be removed
            if (blockchainType == null)
            {
                return null;
            }

            var containerName = OperationExecutionBlobEntity.GetContainerName(blockchainType);
            var blobName = OperationExecutionBlobEntity.GetBlobName(operationId);

            if (!await _blob.HasBlobAsync(containerName, blobName))
            {
                return null;
            }
            
            using (var stream = await _blob.GetAsync(containerName, blobName))
            using (var textReader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(textReader))
            {
                stream.Position = 0;

                return _jsonSerializer.Deserialize<OperationExecutionBlobEntity>(jsonReader);
            }
        }

        private async Task SaveBlobEntityAsync(
            Guid operationId, 
            string blockchainType, 
            OperationExecutionBlobEntity blobEntity)
        {
            var containerName = OperationExecutionBlobEntity.GetContainerName(blockchainType);
            var blobName = OperationExecutionBlobEntity.GetBlobName(operationId);

            using(var stream = new MemoryStream())
            using(var textWriter = new StreamWriter(stream))
            using (var jsonWriter = new JsonTextWriter(textWriter))
            {
                _jsonSerializer.Serialize(jsonWriter, blobEntity);

                await jsonWriter.FlushAsync();
                await textWriter.FlushAsync();
                await stream.FlushAsync();

                stream.Position = 0;

                await _blob.SaveBlobAsync(containerName, blobName, stream);
            }
        }

    }
}

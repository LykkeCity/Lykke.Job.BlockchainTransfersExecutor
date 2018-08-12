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
    public class TransactionExecutionsRepository : ITransactionExecutionsRepository
    {
        private readonly INoSQLTableStorage<TransactionExecutionEntity> _storage;
        private readonly IBlobStorage _blob;
        private readonly JsonSerializer _jsonSerializer;

        public static ITransactionExecutionsRepository Create(IReloadingManager<string> connectionString, ILog log)
        {
            var storage = AzureTableStorage<TransactionExecutionEntity>.Create(
                connectionString,
                "TransactionExecutions",
                log);
            var blob = AzureBlobStorage.Create(connectionString);

            return new TransactionExecutionsRepository(storage, blob);
        }

        private TransactionExecutionsRepository(INoSQLTableStorage<TransactionExecutionEntity> storage, IBlobStorage blob)
        {
            _storage = storage;
            _blob = blob;

            _jsonSerializer = new JsonSerializer();
        }

        public async Task<TransactionExecutionAggregate> GetOrAddAsync(Guid operationId, Func<TransactionExecutionAggregate> newAggregateFactory)
        {
            var partitionKey = TransactionExecutionEntity.GetPartitionKey(operationId);
            var rowKey = TransactionExecutionEntity.GetRowKey(operationId);

            var startedEntity = await _storage.GetOrInsertAsync(
                partitionKey,
                rowKey,
                () =>
                {
                    var newAggregate = newAggregateFactory();

                    return TransactionExecutionEntity.FromDomain(newAggregate);
                });

            var blobEntity = await TryGetBlobEntityAsync(operationId, startedEntity.BlockchainType);
            
            return startedEntity.ToDomain(blobEntity);
        }

        public async Task<TransactionExecutionAggregate> GetAsync(Guid operationId)
        {
            var aggregate = await TryGetAsync(operationId);

            if (aggregate == null)
            {
                throw new InvalidOperationException($"Operation execution with operation ID [{operationId}] is not found");
            }

            return aggregate;
        }

        public async Task<TransactionExecutionAggregate> TryGetAsync(Guid operationId)
        {
            var partitionKey = TransactionExecutionEntity.GetPartitionKey(operationId);
            var rowKey = TransactionExecutionEntity.GetRowKey(operationId);

            var entity = await _storage.GetDataAsync(partitionKey, rowKey);
            
            if (entity != null)
            {
                var blobEntity = await TryGetBlobEntityAsync(operationId, entity.BlockchainType);

                return entity.ToDomain(blobEntity);
            }

            return null;
        }

        public Task SaveAsync(TransactionExecutionAggregate aggregate)
        {
            var entity = TransactionExecutionEntity.FromDomain(aggregate);
            var blobEntity = TransactionExecutionBlobEntity.FromDomain(aggregate);

            return Task.WhenAll
            (
                SaveBlobEntityAsync(aggregate.OperationId, aggregate.BlockchainType, blobEntity),
                _storage.ReplaceAsync(entity)
            );
        }

        private async Task<TransactionExecutionBlobEntity> TryGetBlobEntityAsync(
            Guid operationId,
            string blockchainType)
        {
            var containerName = TransactionExecutionBlobEntity.GetContainerName(blockchainType);
            var blobName = TransactionExecutionBlobEntity.GetBlobName(operationId);

            if (!await _blob.HasBlobAsync(containerName, blobName))
            {
                return null;
            }
            
            using (var stream = await _blob.GetAsync(containerName, blobName))
            using (var textReader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(textReader))
            {
                stream.Position = 0;

                return _jsonSerializer.Deserialize<TransactionExecutionBlobEntity>(jsonReader);
            }
        }

        private async Task SaveBlobEntityAsync(
            Guid operationId, 
            string blockchainType, 
            TransactionExecutionBlobEntity blobEntity)
        {
            var containerName = TransactionExecutionBlobEntity.GetContainerName(blockchainType);
            var blobName = TransactionExecutionBlobEntity.GetBlobName(operationId);

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

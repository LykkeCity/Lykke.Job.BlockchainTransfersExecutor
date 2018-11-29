using System;
using System.IO;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Blob;
using AzureStorage.Tables;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.TransactionExecutions;
using Lykke.SettingsReader;
using Newtonsoft.Json;

namespace Lykke.Job.BlockchainOperationsExecutor.AzureRepositories.TransactionExecutions
{
    [UsedImplicitly]
    public class TransactionExecutionsRepository : ITransactionExecutionsRepository
    {
        private readonly AggregateRepository<TransactionExecutionAggregate, TransactionExecutionEntity> _aggregateRepository;
        private readonly IBlobStorage _blob;
        private readonly JsonSerializer _blobJsonSerializer;
        
        public static ITransactionExecutionsRepository Create(IReloadingManager<string> connectionString, ILogFactory logFactory)
        {
            var storage = AzureTableStorage<TransactionExecutionEntity>.Create(
                connectionString,
                "TransactionExecutions",
                logFactory);
            var blob = AzureBlobStorage.Create(connectionString);

            return new TransactionExecutionsRepository(storage, blob);
        }

        private TransactionExecutionsRepository(
            INoSQLTableStorage<TransactionExecutionEntity> storage,
            IBlobStorage blob)
        {
            _aggregateRepository = new AggregateRepository<TransactionExecutionAggregate, TransactionExecutionEntity>(
                storage,
                mapAggregateToEntity: TransactionExecutionEntity.FromDomain,
                mapEntityToAggregate: async entity =>
                {
                    var blobEntity = await TryGetBlobEntityAsync(entity.TransactionId, entity.BlockchainType);
                    
                    return entity.ToDomain(blobEntity);
                });
            _blob = blob;

            _blobJsonSerializer = new JsonSerializer();
        }

        public Task<TransactionExecutionAggregate> GetOrAddAsync(Guid transactionId, Func<TransactionExecutionAggregate> newAggregateFactory)
        {
            return _aggregateRepository.GetOrAddAsync(transactionId, newAggregateFactory);
        }

        public Task<TransactionExecutionAggregate> GetAsync(Guid transactionId)
        {
            return _aggregateRepository.GetAsync(transactionId);
        }

        public Task<TransactionExecutionAggregate> TryGetAsync(Guid transactionId)
        {
            return _aggregateRepository.TryGetAsync(transactionId);
        }

        public Task SaveAsync(TransactionExecutionAggregate aggregate)
        {
            var blobEntity = TransactionExecutionBlobEntity.FromDomain(aggregate);

            return Task.WhenAll
            (
                SaveBlobEntityAsync(aggregate.TransactionId, aggregate.BlockchainType, blobEntity),
                _aggregateRepository.SaveAsync(aggregate)
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

                return _blobJsonSerializer.Deserialize<TransactionExecutionBlobEntity>(jsonReader);
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
                _blobJsonSerializer.Serialize(jsonWriter, blobEntity);

                await jsonWriter.FlushAsync();
                await textWriter.FlushAsync();
                await stream.FlushAsync();

                stream.Position = 0;

                await _blob.SaveBlobAsync(containerName, blobName, stream);
            }
        }

    }
}

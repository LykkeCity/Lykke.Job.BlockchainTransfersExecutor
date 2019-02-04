using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Blob;
using AzureStorage.Tables;
using Lykke.Common.Log;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.TransactionExecutions;
using Lykke.SettingsReader;
using Newtonsoft.Json;

namespace Lykke.Job.BlockchainOperationsExecutor.AzureRepositories.TransactionExecutions
{
    public class CommandHandlerEventRepository: ICommandHandlerEventRepository
    {
        private readonly IReadOnlyDictionary<string, Type> _keyTypesDictionary;
        private readonly IReadOnlyDictionary<Type, string> _typeKeyDictionary;
        private readonly INoSQLTableStorage<CommandHandlerEventEntity> _storage;
        private readonly IBlobStorage _blob;
        private readonly JsonSerializer _blobJsonSerializer;

        private CommandHandlerEventRepository(ICollection<(string eventKey, Type eventType)> typeKeys, 
            INoSQLTableStorage<CommandHandlerEventEntity> storage,
            IBlobStorage blob)
        {
            _keyTypesDictionary = typeKeys.ToDictionary(p => p.eventKey, p => p.eventType);
            _typeKeyDictionary = typeKeys.ToDictionary(p => p.eventType, p => p.eventKey);

            _storage = storage;
            _blob = blob;
            _blobJsonSerializer = new JsonSerializer();
        }

        public static ICommandHandlerEventRepository Create(IReloadingManager<string> connectionString,
            ILogFactory logFactory,
            ICollection<(string eventKey, Type eventType)> typeKeys)
        {
            var storage = AzureTableStorage<CommandHandlerEventEntity>.Create(
                connectionString,
                "CommandHandlerEvents",
                logFactory);

            var blob = AzureBlobStorage.Create(connectionString);

            return new CommandHandlerEventRepository(typeKeys, storage, blob);
        }

        public async Task<object> TryGetEventAsync(Guid aggregateId, string commandHandlerId)
        {
            var entity = await _storage.GetDataAsync(CommandHandlerEventEntity.GeneratePartitionKey(aggregateId),
                CommandHandlerEventEntity.GenerateRowKey(commandHandlerId));

            if (entity != null)
            {
                if (!_keyTypesDictionary.ContainsKey(entity.EventTypeKey))
                {
                    throw new ArgumentException($"Unable to map type with key {entity.EventTypeKey}." +
                                                "Configured keys: " +
                                                $"{string.Join(", ", _keyTypesDictionary.Select(p => $"{p.Key}:{p.Value.Name}"))}");
                }

                var containerName = BuildBlobContainerName(commandHandlerId);
                var fileName = BuildBlobFileName(aggregateId);

                if (await _blob.HasBlobAsync(containerName, fileName))
                {
                    throw new ArgumentException("Unable to find blob data");
                }

                return JsonConvert.DeserializeObject(
                    await _blob.GetAsTextAsync(containerName, fileName),
                    _keyTypesDictionary[entity.EventTypeKey]);
            }

            return null;
        }

        public async Task InsertEventAsync<T>(Guid aggregateId, string commandHandlerId, T eventData)
        {
            if (!_typeKeyDictionary.ContainsKey(typeof(T)))
            {
                throw new ArgumentException($"Using {typeof(T).Name} not configured. " +
                                            "Configured types: " +
                                            $"{string.Join(", ", _typeKeyDictionary.Select(p => $"{p.Value}:{p.Key.Name}"))}",
                    nameof(eventData));
            }

            var correlationId = Guid.NewGuid();

            try
            {
                await _storage.InsertAsync(new CommandHandlerEventEntity
                {
                    AggregateId = aggregateId,
                    CommandHandlerId = commandHandlerId,
                    EventTypeKey = _typeKeyDictionary[typeof(T)],
                    PartitionKey = CommandHandlerEventEntity.GeneratePartitionKey(aggregateId),
                    RowKey = CommandHandlerEventEntity.GenerateRowKey(commandHandlerId),
                    CorrelationId = correlationId
                });

                await SaveBlobEntityAsync(aggregateId, commandHandlerId, eventData);
            }
            catch (Exception)
            {
                await _storage.DeleteIfExistAsync(CommandHandlerEventEntity.GeneratePartitionKey(aggregateId),
                    CommandHandlerEventEntity.GenerateRowKey(commandHandlerId),
                    ent => ent.CorrelationId == correlationId);

                throw;
            }
        }

        private static string BuildBlobContainerName(string commandHandlerId)
        {
            return $"command-handler-event-data-{commandHandlerId}";
        }

        private static string BuildBlobFileName(Guid aggregateId)
        {
            return aggregateId.ToString();
        }

        private async Task SaveBlobEntityAsync(
            Guid aggregateId,
            string commandHandlerId,
            object data)
        {
            var containerName = BuildBlobContainerName(commandHandlerId);
            var blobName = BuildBlobFileName(aggregateId);

            using (var stream = new MemoryStream())
            using (var textWriter = new StreamWriter(stream))
            using (var jsonWriter = new JsonTextWriter(textWriter))
            {
                _blobJsonSerializer.Serialize(jsonWriter, data);

                await jsonWriter.FlushAsync();
                await textWriter.FlushAsync();
                await stream.FlushAsync();

                stream.Position = 0;
                
                await _blob.SaveBlobAsync(containerName, blobName, stream);
            }
        }
    }
}

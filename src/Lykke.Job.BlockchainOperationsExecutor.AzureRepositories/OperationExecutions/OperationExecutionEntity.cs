using System;
using System.Linq;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.Job.BlockchainOperationsExecutor.AzureRepositories.TransactionExecutions;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.OperationExecutions;

namespace Lykke.Job.BlockchainOperationsExecutor.AzureRepositories.OperationExecutions
{
    internal class OperationExecutionEntity : AzureTableEntity
    {
        #region Fields

        // ReSharper disable MemberCanBePrivate.Global

        public OperationExecutionState State { get; set; }
        public OperationExecutionResult? Result { get; set; }

        public DateTime StartMoment { get; set; }
        public DateTime? ActiveTransactionIdGenerationMoment { get; set; }
        public DateTime? ActiveTransactionStartMoment { get; set; }
        public DateTime? TransactionExecutionRepeatRequestMoment { get; set; }
        public DateTime? ActiveTransactionClearingMoment { get; set; }
        public DateTime? TransactionFinishMoment { get; set; }
        public DateTime? FinishMoment { get; set; }

        public Guid OperationId { get; set; }

        public string FromAddress { get; set; }
        [JsonValueSerializer]
        public TransactionOutputEntity[] Outputs { get; set; }
        public string AssetId { get; set; }
        public bool IncludeFee { get; set; }
        public string BlockchainType { get; set; }
        public string BlockchainAssetId { get; set; }
        public OperationExecutionEndpointsConfiguration EndpointsConfiguration { get; set; }

        public Guid? ActiveTransactionId { get; set; }
        public int ActiveTransactionNumber { get; set; }
        [JsonValueSerializer]
        public TransactionOutputEntity[] TransactionOutputs { get; set; }
        public long TransactionBlock { get; set; }
        public decimal TransactionFee { get; set; }
        public string TransactionHash { get; set; }
        public string Error { get; set; }

        public RebuildConfirmationResult RebuildConfirmationResult { get; set; }

        // ReSharper restore MemberCanBePrivate.Global

        #endregion


        #region Conversion

        public static OperationExecutionEntity FromDomain(OperationExecutionAggregate aggregate)
        {
            return new OperationExecutionEntity
            {
                ETag = aggregate.Version,
                PartitionKey = AggregateKeysBuilder.BuildPartitionKey(aggregate.OperationId),
                RowKey = AggregateKeysBuilder.BuildRowKey(aggregate.OperationId),
                State = aggregate.State,
                Result = aggregate.Result,
                StartMoment = aggregate.StartMoment,
                ActiveTransactionIdGenerationMoment = aggregate.ActiveTransactionIdGenerationMoment,
                ActiveTransactionStartMoment = aggregate.ActiveTransactionStartMoment,
                TransactionExecutionRepeatRequestMoment = aggregate.TransactionExecutionRepeatRequestMoment,
                ActiveTransactionClearingMoment = aggregate.ActiveTransactionClearingMoment,
                TransactionFinishMoment = aggregate.TransactionFinishMoment,
                FinishMoment = aggregate.FinishMoment,
                OperationId = aggregate.OperationId,
                BlockchainType = aggregate.BlockchainType,
                FromAddress = aggregate.FromAddress,
                Outputs = aggregate.Outputs
                    .Select(TransactionOutputEntity.FromDomain)
                    .ToArray(),
                AssetId = aggregate.AssetId,
                IncludeFee = aggregate.IncludeFee,
                BlockchainAssetId = aggregate.BlockchainAssetId,
                EndpointsConfiguration = aggregate.EndpointsConfiguration,
                ActiveTransactionId = aggregate.ActiveTransactionId,
                ActiveTransactionNumber = aggregate.ActiveTransactionNumber,
                TransactionOutputs = aggregate.TransactionOutputs?
                    .Select(TransactionOutputEntity.FromDomain)
                    .ToArray(),
                TransactionBlock = aggregate.TransactionBlock,
                TransactionFee = aggregate.TransactionFee,
                TransactionHash = aggregate.TransactionHash,
                Error = aggregate.Error,
                RebuildConfirmationResult = aggregate.RebuildConfirmationResult
            };
        }

        public OperationExecutionAggregate ToDomain()
        {
            return OperationExecutionAggregate.Restore(
                ETag,
                State,
                Result,
                StartMoment,
                ActiveTransactionIdGenerationMoment,
                ActiveTransactionStartMoment,
                TransactionExecutionRepeatRequestMoment,
                ActiveTransactionClearingMoment,
                TransactionFinishMoment,
                FinishMoment,
                OperationId,
                FromAddress,
                Outputs
                    .Select(x => x.ToDomain())
                    .ToArray(),
                AssetId,
                IncludeFee,
                BlockchainType,
                BlockchainAssetId,
                EndpointsConfiguration,
                ActiveTransactionId,
                ActiveTransactionNumber,
                TransactionOutputs?
                    .Select(o => o.ToDomain())
                    .ToArray(),
                TransactionBlock,
                TransactionFee,
                TransactionHash,
                Error,
                RebuildConfirmationResult);
        }

        #endregion        
    }
}

using System;
using System.Linq;
using JetBrains.Annotations;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.TransactionExecutions;

namespace Lykke.Job.BlockchainOperationsExecutor.AzureRepositories.TransactionExecutions
{
    internal class TransactionExecutionEntity : AzureTableEntity
    {
        #region Fields

        // ReSharper disable MemberCanBePrivate.Global

        public TransactionExecutionState State { get; set; }
        public TransactionExecutionResult? Result { get; set; }

        public DateTime StartMoment { get; set; }
        public DateTime? SourceAddressLockingMoment { get; set; }
        public DateTime? BuildingMoment { get; set; }
        public DateTime? SigningMoment { get; set; }
        public DateTime? BroadcastingMoment { get; set; }
        public DateTime? FinishMoment { get; set; }
        public DateTime? SourceAddressReleaseMoment { get; set; }
        public DateTime? ClearedMoment { get; set; }
        public DateTime? BuildingFailureMoment { get; set; }
        public DateTime? BroadcastinFailureMoment { get; set; }
        public DateTime? WaitingForEndingStartMoment { get; set; }
        public DateTime? WaitingForEndingFailureMoment { get; set; }

        public Guid OperationId { get; set; }
        public Guid TransactionId { get; set; }
        public int TransactionNumber {get; set; }
        public string BlockchainType { get; set; }
        public string FromAddress { get; set; }
        public string FromAddressContext { get; set; }
        [JsonValueSerializer]
        public TransactionOutputEntity[] Outputs { get; set; }
        public string AssetId { get; set; }
        public bool IncludeFee { get; set; }
        public string BlockchainAssetId { get; set; }
        [JsonValueSerializer]
        public TransactionOutputEntity[] TransactionOutputs { get; set; }
        public string Hash { get; set; }
        public decimal? Fee { get; set; }
        public string Error { get; set; }
        public long? Block { get; set; }

        // ReSharper restore MemberCanBePrivate.Global

        #endregion

        
        #region Conversion

        public static TransactionExecutionEntity FromDomain(TransactionExecutionAggregate aggregate)
        {
            return new TransactionExecutionEntity
            {
                ETag = aggregate.Version,
                PartitionKey = AggregateKeysBuilder.BuildPartitionKey(aggregate.TransactionId),
                RowKey = AggregateKeysBuilder.BuildRowKey(aggregate.TransactionId),
                State = aggregate.State,
                Result = aggregate.Result,
                StartMoment = aggregate.StartMoment,
                SourceAddressLockingMoment = aggregate.SourceAddressLockingMoment,
                BuildingMoment = aggregate.BuildingMoment,
                SigningMoment = aggregate.SigningMoment,
                BroadcastingMoment = aggregate.BroadcastingMoment,
                FinishMoment = aggregate.FinishMoment,
                BuildingFailureMoment = aggregate.BuildingFailureMoment,
                BroadcastinFailureMoment = aggregate.BroadcastinFailureMoment,
                WaitingForEndingStartMoment = aggregate.WaitingForEndingStartMoment,
                WaitingForEndingFailureMoment = aggregate.WaitingForEndingFailureMoment,
                SourceAddressReleaseMoment = aggregate.SourceAddressReleasingMoment,
                ClearedMoment = aggregate.ClearingMoment,
                OperationId = aggregate.OperationId,
                TransactionId = aggregate.TransactionId,
                TransactionNumber = aggregate.TransactionNumber,
                BlockchainType = aggregate.BlockchainType,
                FromAddress = aggregate.FromAddress,
                FromAddressContext = aggregate.FromAddressContext,
                Outputs = aggregate.Outputs
                    .Select(TransactionOutputEntity.FromDomain)
                    .ToArray(),
                AssetId = aggregate.AssetId,
                IncludeFee = aggregate.IncludeFee,
                BlockchainAssetId = aggregate.BlockchainAssetId,
                TransactionOutputs = aggregate.TransactionOutputs?
                    .Select(TransactionOutputEntity.FromDomain)
                    .ToArray(),
                Hash = aggregate.Hash,
                Fee = aggregate.Fee,
                Error = aggregate.Error,
                Block = aggregate.Block
            };
        }

        public TransactionExecutionAggregate ToDomain([CanBeNull] TransactionExecutionBlobEntity blobData)
        {
            return TransactionExecutionAggregate.Restore(
                ETag,
                State,
                Result,
                StartMoment,
                SourceAddressLockingMoment,
                BuildingMoment,
                SigningMoment,
                BroadcastingMoment,
                FinishMoment,
                SourceAddressReleaseMoment,
                ClearedMoment,
                BuildingFailureMoment,
                BroadcastinFailureMoment,
                WaitingForEndingStartMoment,
                WaitingForEndingFailureMoment,
                OperationId,
                TransactionId,
                TransactionNumber,
                BlockchainType,
                FromAddress,
                FromAddressContext,
                Outputs
                    .Select(x => x.ToDomain())
                    .ToArray(),
                AssetId,
                IncludeFee,
                blobData?.TransactionContext,
                BlockchainAssetId,
                blobData?.SignedTransaction,
                TransactionOutputs?
                    .Select(o => o.ToDomain())
                    .ToArray(),
                Block,
                Fee,
                Hash,
                Error);
        }

        #endregion
    }
}

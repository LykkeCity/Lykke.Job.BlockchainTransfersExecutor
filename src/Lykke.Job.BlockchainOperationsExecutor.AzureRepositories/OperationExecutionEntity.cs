using System;
using Common;
using JetBrains.Annotations;
using Lykke.AzureStorage.Tables;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain;

namespace Lykke.Job.BlockchainOperationsExecutor.AzureRepositories
{
    internal class OperationExecutionEntity : AzureTableEntity
    {
        #region Fields

        [UsedImplicitly]
        public OperationExecutionState State { get; set; }

        [UsedImplicitly]
        public DateTime StartMoment { get; set; }
        [UsedImplicitly]
        public DateTime? TransactionBuildingMoment { get; set; }
        [UsedImplicitly]
        public DateTime? TransactionSigningMoment { get; set; }
        [UsedImplicitly]
        public DateTime? TransactionBroadcastingMoment { get; set; }
        [UsedImplicitly]
        public DateTime? TransactionFinishMoment { get; set; }
        [UsedImplicitly]
        public DateTime? SourceAddressReleaseMoment { get; set; }

        [UsedImplicitly]
        public Guid OperationId { get; set; }
        [UsedImplicitly]
        public string BlockchainType { get; set; }
        [UsedImplicitly]
        public string FromAddress { get; set; }
        [UsedImplicitly]
        public string ToAddress { get; set; }
        [UsedImplicitly]
        public string AssetId { get; set; }
        [UsedImplicitly]
        public decimal Amount { get; set; }
        [UsedImplicitly]
        public bool IncludeFee { get; set; }
        [UsedImplicitly]
        public string TransactionContext { get; set; }
        [UsedImplicitly]
        public string BlockchainAssetId { get; set; }
        [UsedImplicitly]
        public string SignedTransaction { get; set; }
        [UsedImplicitly]
        public string TransactionHash { get; set; }
        [UsedImplicitly]
        public DateTime? TransactionTimestamp { get; set; }
        [UsedImplicitly]
        public decimal? Fee { get; set; }
        [UsedImplicitly]
        public string TransactionError { get; set; }

        #endregion

        
        #region Keys

        public static string GetPartitionKey(Guid operationId)
        {
            // Use hash to distribute all records to the different partitions
            var hash = operationId.ToString().CalculateHexHash32(3);

            return $"{hash}";
        }

        public static string GetRowKey(Guid operationId)
        {
            return $"{operationId:D}";
        }

        #endregion

        
        #region Conversion

        public static OperationExecutionEntity FromDomain(OperationExecutionAggregate aggregate)
        {
            return new OperationExecutionEntity
            {
                ETag = string.IsNullOrEmpty(aggregate.Version) ? "*" : aggregate.Version,
                PartitionKey = GetPartitionKey(aggregate.OperationId),
                RowKey = GetRowKey(aggregate.OperationId),
                State = aggregate.State,
                StartMoment = aggregate.StartMoment,
                TransactionBuildingMoment = aggregate.TransactionBuildingMoment,
                TransactionSigningMoment = aggregate.TransactionSigningMoment,
                TransactionBroadcastingMoment = aggregate.TransactionBroadcastingMoment,
                TransactionFinishMoment = aggregate.TransactionFinishMoment,
                SourceAddressReleaseMoment = aggregate.SourceAddressReleaseMoment,
                OperationId = aggregate.OperationId,
                BlockchainType = aggregate.BlockchainType,
                FromAddress = aggregate.FromAddress,
                ToAddress = aggregate.ToAddress,
                AssetId = aggregate.AssetId,
                Amount = aggregate.Amount,
                IncludeFee = aggregate.IncludeFee,
                TransactionContext = aggregate.TransactionContext,
                BlockchainAssetId = aggregate.BlockchainAssetId,
                SignedTransaction = aggregate.SignedTransaction,
                TransactionHash = aggregate.TransactionHash,
                TransactionTimestamp = aggregate.TransactionTimestamp,
                Fee = aggregate.Fee,
                TransactionError = aggregate.TransactionError
            };
        }

        public OperationExecutionAggregate ToDomain()
        {
            return OperationExecutionAggregate.Restore(
                ETag,
                State,
                StartMoment,
                TransactionBuildingMoment,
                TransactionSigningMoment,
                TransactionBroadcastingMoment,
                TransactionFinishMoment,
                SourceAddressReleaseMoment,
                OperationId,
                BlockchainType,
                FromAddress,
                ToAddress,
                AssetId,
                Amount,
                IncludeFee,
                TransactionContext,
                BlockchainAssetId,
                SignedTransaction,
                TransactionHash,
                TransactionTimestamp,
                Fee,
                TransactionError);
        }

        #endregion
    }
}

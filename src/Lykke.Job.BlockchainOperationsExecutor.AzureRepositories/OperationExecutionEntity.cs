using System;
using Common;
using Lykke.AzureStorage.Tables;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain;

namespace Lykke.Job.BlockchainOperationsExecutor.AzureRepositories
{
    internal class OperationExecutionEntity : AzureTableEntity
    {
        #region Fields

        // ReSharper disable MemberCanBePrivate.Global

        public OperationExecutionState State { get; set; }
        public OperationExecutionResult Result { get; set; }

        public DateTime StartMoment { get; set; }
        public DateTime? TransactionBuildingMoment { get; set; }
        public DateTime? TransactionSigningMoment { get; set; }
        public DateTime? TransactionBroadcastingMoment { get; set; }
        public DateTime? TransactionFinishMoment { get; set; }
        public DateTime? SourceAddressReleaseMoment { get; set; }
        public DateTime? BroadcastedTransactionForgetMoment { get; set; }

        public Guid OperationId { get; set; }
        public string BlockchainType { get; set; }
        public string FromAddress { get; set; }
        public string FromAddressContext { get; set; }
        public string ToAddress { get; set; }
        public string AssetId { get; set; }
        public decimal Amount { get; set; }
        public bool IncludeFee { get; set; }
        public string TransactionContext { get; set; }
        public string BlockchainAssetId { get; set; }
        public string SignedTransaction { get; set; }
        public string TransactionHash { get; set; }
        public decimal? Fee { get; set; }
        public string TransactionError { get; set; }
        public long? TransactionBlock { get; set; }

        // ReSharper restore MemberCanBePrivate.Global

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
                Result = aggregate.Result,
                StartMoment = aggregate.StartMoment,
                TransactionBuildingMoment = aggregate.TransactionBuildingMoment,
                TransactionSigningMoment = aggregate.TransactionSigningMoment,
                TransactionBroadcastingMoment = aggregate.TransactionBroadcastingMoment,
                TransactionFinishMoment = aggregate.TransactionFinishMoment,
                SourceAddressReleaseMoment = aggregate.SourceAddressReleaseMoment,
                BroadcastedTransactionForgetMoment = aggregate.BroadcastedTransactionForgetMoment,
                OperationId = aggregate.OperationId,
                BlockchainType = aggregate.BlockchainType,
                FromAddress = aggregate.FromAddress,
                FromAddressContext = aggregate.FromAddressContext,
                ToAddress = aggregate.ToAddress,
                AssetId = aggregate.AssetId,
                Amount = aggregate.Amount,
                IncludeFee = aggregate.IncludeFee,
                TransactionContext = aggregate.TransactionContext,
                BlockchainAssetId = aggregate.BlockchainAssetId,
                SignedTransaction = aggregate.SignedTransaction,
                TransactionHash = aggregate.TransactionHash,
                Fee = aggregate.Fee,
                TransactionError = aggregate.TransactionError,
                TransactionBlock = aggregate.TransactionBlock
            };
        }

        public OperationExecutionAggregate ToDomain()
        {
            return OperationExecutionAggregate.Restore(
                ETag,
                State,
                Result,
                StartMoment,
                TransactionBuildingMoment,
                TransactionSigningMoment,
                TransactionBroadcastingMoment,
                TransactionFinishMoment,
                SourceAddressReleaseMoment,
                BroadcastedTransactionForgetMoment,
                OperationId,
                BlockchainType,
                FromAddress,
                FromAddressContext,
                ToAddress,
                AssetId,
                Amount,
                IncludeFee,
                TransactionContext,
                BlockchainAssetId,
                SignedTransaction,
                TransactionHash,
                Fee,
                TransactionError,
                TransactionBlock);
        }

        #endregion
    }
}

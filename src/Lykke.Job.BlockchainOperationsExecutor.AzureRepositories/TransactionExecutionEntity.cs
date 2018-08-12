using System;
using Common;
using JetBrains.Annotations;
using Lykke.AzureStorage.Tables;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain;

namespace Lykke.Job.BlockchainOperationsExecutor.AzureRepositories
{
    internal class TransactionExecutionEntity : AzureTableEntity
    {
        #region Fields

        // ReSharper disable MemberCanBePrivate.Global

        public TransactionExecutionState State { get; set; }
        public TransactionExecutionResult? Result { get; set; }

        public DateTime StartMoment { get; set; }
        public DateTime? BuildingMoment { get; set; }
        public DateTime? SigningMoment { get; set; }
        public DateTime? BroadcastingMoment { get; set; }
        public DateTime? FinishMoment { get; set; }
        public DateTime? SourceAddressReleaseMoment { get; set; }
        public DateTime? ClearedMoment { get; set; }

        public Guid OperationId { get; set; }
        public Guid TransactionId { get; set; }
        public string BlockchainType { get; set; }
        public string FromAddress { get; set; }
        public string FromAddressContext { get; set; }
        public string ToAddress { get; set; }
        public string AssetId { get; set; }
        public decimal Amount { get; set; }
        public bool IncludeFee { get; set; }
        public string BlockchainAssetId { get; set; }
        public string Hash { get; set; }
        public decimal? Fee { get; set; }
        public string Error { get; set; }
        public long? Block { get; set; }
        public bool WasBroadcasted { get; set; }

        // ReSharper restore MemberCanBePrivate.Global

        #endregion


        #region Keys

        public static string GetPartitionKey(Guid transactionId)
        {
            // Use hash to distribute all records to the different partitions
            var hash = transactionId.ToString().CalculateHexHash32(3);

            return $"{hash}";
        }

        public static string GetRowKey(Guid transactionId)
        {
            return $"{transactionId:D}";
        }

        #endregion

        
        #region Conversion

        public static TransactionExecutionEntity FromDomain(TransactionExecutionAggregate aggregate)
        {
            return new TransactionExecutionEntity
            {
                ETag = aggregate.Version,
                PartitionKey = GetPartitionKey(aggregate.TransactionId),
                RowKey = GetRowKey(aggregate.TransactionId),
                State = aggregate.State,
                Result = aggregate.Result,
                StartMoment = aggregate.StartMoment,
                BuildingMoment = aggregate.BuildingMoment,
                SigningMoment = aggregate.SigningMoment,
                BroadcastingMoment = aggregate.BroadcastingMoment,
                FinishMoment = aggregate.FinishMoment,
                SourceAddressReleaseMoment = aggregate.SourceAddressReleasingMoment,
                ClearedMoment = aggregate.ClearingMoment,
                OperationId = aggregate.OperationId,
                TransactionId = aggregate.TransactionId,
                BlockchainType = aggregate.BlockchainType,
                FromAddress = aggregate.FromAddress,
                FromAddressContext = aggregate.FromAddressContext,
                ToAddress = aggregate.ToAddress,
                AssetId = aggregate.AssetId,
                Amount = aggregate.Amount,
                IncludeFee = aggregate.IncludeFee,
                BlockchainAssetId = aggregate.BlockchainAssetId,
                Hash = aggregate.Hash,
                Fee = aggregate.Fee,
                Error = aggregate.Error,
                Block = aggregate.Block,
                WasBroadcasted = aggregate.WasBroadcasted
            };
        }

        public TransactionExecutionAggregate ToDomain([CanBeNull] TransactionExecutionBlobEntity blobData)
        {
            return TransactionExecutionAggregate.Restore(
                ETag,
                State,
                Result,
                StartMoment,
                BuildingMoment,
                SigningMoment,
                BroadcastingMoment,
                FinishMoment,
                SourceAddressReleaseMoment,
                ClearedMoment,
                OperationId,
                TransactionId,
                BlockchainType,
                FromAddress,
                FromAddressContext,
                ToAddress,
                AssetId,
                Amount,
                IncludeFee,
                blobData?.TransactionContext,
                BlockchainAssetId,
                blobData?.SignedTransaction,
                Hash,
                Fee,
                Error,
                Block,
                WasBroadcasted);
        }

        #endregion
    }
}

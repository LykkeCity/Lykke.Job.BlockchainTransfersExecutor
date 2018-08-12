using System;

namespace Lykke.Job.BlockchainOperationsExecutor.Core.Domain
{
    public class OperationExecutionAggregate
    {
        public string Version { get; }

        public OperationExecutionState State { get; }
        public OperationExecutionResult? Result { get; private set; }

        public DateTime StartMoment { get; }

        public Guid OperationId { get; }

        public string FromAddress { get; }
        public string ToAddress { get; }
        public string AssetId { get; }
        public decimal Amount { get; }
        public bool IncludeFee { get; }
        public string BlockchainType { get; }
        public string BlockchainAssetId { get; }

        public string TransactionHash { get; private set; }
        public decimal? TransactionFee { get; private set; }
        public long? TransactionBlock { get; private set; }
        public string Error { get; private set; }
    }

    public class TransactionExecutionAggregate
    {
        public string Version { get; }

        public TransactionExecutionState State { get; set; }
        public TransactionExecutionResult? Result { get; private set; }

        public DateTime StartMoment { get; }
        public DateTime? BuildingMoment { get; private set; }
        public DateTime? SigningMoment { get; private set; }
        public DateTime? BroadcastingMoment { get; private set; }
        public DateTime? FinishMoment { get; private set; }
        public DateTime? SourceAddressReleasingMoment { get; private set; }
        public DateTime? ClearingMoment { get; private set; }

        public Guid OperationId { get; }
        public Guid TransactionId { get; }
        public string FromAddress { get; }
        public string ToAddress { get; }
        public string AssetId { get; }
        public decimal Amount { get; }
        public bool IncludeFee { get; }

        public string BlockchainType { get; }
        public string BlockchainAssetId { get; }

        public string Context { get; private set; }
        public string SignedTransaction { get; private set; }
        public string Hash { get; private set; }
        public decimal? Fee { get; private set; }
        public string Error { get; private set; }
        public string FromAddressContext { get; private set; }
        public long? Block { get; private set; }

        public bool WasBroadcasted { get; private set; }

        private TransactionExecutionAggregate(
            string version,
            Guid operationId, 
            Guid transactionId,
            string fromAddress, 
            string toAddress, 
            string blockchainType,
            string blockchainAssetId,
            string assetId, 
            decimal amount, 
            bool includeFee,
            TransactionExecutionState state,
            DateTime startMoment)
        {
            Version = version;
            OperationId = operationId;
            TransactionId = transactionId;
            FromAddress = fromAddress;
            ToAddress = toAddress;
            BlockchainType = blockchainType;
            BlockchainAssetId = blockchainAssetId;
            AssetId = assetId;
            Amount = amount;
            IncludeFee = includeFee;
            State = state;
            StartMoment = startMoment;
        }

        public static TransactionExecutionAggregate CreateNew(
            Guid operationId,
            Guid transactionId,
            string fromAddress,
            string toAddress,
            string blockchainType,
            string blockchainAssetId,
            string assetId,
            decimal amount,
            bool includeFee)
        {
            return new TransactionExecutionAggregate(
                "*",
                operationId,
                transactionId,
                fromAddress,
                toAddress,
                blockchainType,
                blockchainAssetId,
                assetId,
                amount,
                includeFee,
                TransactionExecutionState.Started,
                DateTime.UtcNow);
        }

        public static TransactionExecutionAggregate Restore(
            string version,
            TransactionExecutionState state,
            TransactionExecutionResult? result,
            DateTime startMoment,
            DateTime? buildingMoment,
            DateTime? signingMoment,
            DateTime? broadcastingMoment,
            DateTime? finishMoment,
            DateTime? sourceAddressReleasingMoment,
            DateTime? clearingMoment,
            Guid operationId,
            Guid transactionId,
            string blockchainType,
            string fromAddress,
            string fromAddressContext,
            string toAddress,
            string assetId,
            decimal amount,
            bool includeFee,
            string context,
            string blockchainAssetId,
            string signedTransaction,
            string hash,
            decimal? fee,
            string error,
            long? block,
            bool wasBroadcasted)
        {
            return new TransactionExecutionAggregate(
                version,
                operationId,
                transactionId,
                fromAddress,
                toAddress,
                blockchainType,
                blockchainAssetId,
                assetId,
                amount,
                includeFee,
                state,
                startMoment)
            {
                Result = result,
                BuildingMoment = buildingMoment,
                SigningMoment = signingMoment,
                BroadcastingMoment = broadcastingMoment,
                FinishMoment = finishMoment,
                SourceAddressReleasingMoment = sourceAddressReleasingMoment,
                ClearingMoment = clearingMoment,
                FromAddressContext = fromAddressContext,
                Context = context,
                SignedTransaction = signedTransaction,
                Hash = hash,
                Fee = fee,
                Error = error,
                Block = block,
                WasBroadcasted = wasBroadcasted,
            };
        }
        
        public bool OnBuilt(string fromAddressContext, string transactionContext)
        {
            FromAddressContext = fromAddressContext;
            Context = transactionContext;

            BuildingMoment = DateTime.UtcNow;

            return true;
        }

        public bool OnBuildingRejected()
        {
            // If transaction building is rejected, then address lock has been captured.
            // This is a redundant operation execution thread and main operation execution thread is probably
            // went futher.
            // So we should return true, if lock should be already released. We don't need
            // to switch state since this is just redundant operation execution thread.

            // Lock should be released right after broadcasting
            return !WasBroadcasted;
        }

        public bool OnSigned(string signedTransaction)
        {
            SignedTransaction = signedTransaction;

            SigningMoment = DateTime.UtcNow;

            return true;
        }

        public bool OnBroadcasted()
        {
            BroadcastingMoment = DateTime.UtcNow;
            WasBroadcasted = true;

            return true;
        }

        public bool OnSourceAddressLockReleased()
        {
            SourceAddressReleasingMoment = DateTime.UtcNow;

            return true;
        }

        public bool OnCompleted(string hash, long block, decimal fee)
        {
            Hash = hash;
            Block = block;
            Fee = fee;

            Result = TransactionExecutionResult.Completed;

            FinishMoment = DateTime.UtcNow;

            return true;
        }

        public bool OnFailed(string error)
        {
            Error = error;

            Result = TransactionExecutionResult.Failure;

            FinishMoment = DateTime.UtcNow;

            return true;
        }
        
        public bool OnCleared()
        {
            ClearingMoment = DateTime.UtcNow;

            return true;
        }
    }
}

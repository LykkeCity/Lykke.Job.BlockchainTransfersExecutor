using System;

namespace Lykke.Job.BlockchainOperationsExecutor.Core.Domain
{
    public class OperationExecutionAggregate
    {
        public string Version { get; }

        public OperationExecutionState State { get; set; }
        public OperationExecutionResult Result { get; private set; }

        public DateTime StartMoment { get; }
        public DateTime? TransactionBuildingMoment { get; private set; }
        public DateTime? TransactionSigningMoment { get; private set; }
        public DateTime? TransactionBroadcastingMoment { get; private set; }
        public DateTime? TransactionFinishMoment { get; private set; }
        public DateTime? SourceAddressReleaseMoment { get; private set; }
        public DateTime? BroadcastedTransactionForgetMoment { get; private set; }

        public Guid OperationId { get; }
        public string FromAddress { get; }
        public string ToAddress { get; }
        public string AssetId { get; }
        public decimal Amount { get; }
        public bool IncludeFee { get; }

        // TODO: Should be made read-only property, when obsolete properties will be removed from the OnTransactionBuilt  
        public string BlockchainType { get; private set; }
        // TODO: Should be made read-only property, when obsolete properties will be removed from the OnTransactionBuilt  
        public string BlockchainAssetId { get; private set; }

        public string TransactionContext { get; private set; }
        public string SignedTransaction { get; private set; }
        public string TransactionHash { get; private set; }
        public decimal? Fee { get; private set; }
        public string TransactionError { get; private set; }
        public string FromAddressContext { get; private set; }
        public long? TransactionBlock { get; private set; }

        public bool WasBroadcasted { get; private set; }

        private OperationExecutionAggregate(
            Guid operationId, 
            string fromAddress, 
            string toAddress, 
            string blockchainType,
            string blockchainAssetId,
            string assetId, 
            decimal amount, 
            bool includeFee)
        {
            StartMoment = DateTime.UtcNow;

            OperationId = operationId;
            FromAddress = fromAddress;
            ToAddress = toAddress;
            BlockchainType = blockchainType;
            BlockchainAssetId = blockchainAssetId;
            AssetId = assetId;
            Amount = amount;
            IncludeFee = includeFee;

            State = OperationExecutionState.Started;
            Result = OperationExecutionResult.Unknown;
        }

        private OperationExecutionAggregate(
            string version, 
            OperationExecutionState state,
            OperationExecutionResult result,
            DateTime startMoment,
            DateTime? transactionBuildingMoment,
            DateTime? transactionSigningMoment,
            DateTime? transactionBroadcastingMoment,
            DateTime? transactionFinishMoment,
            DateTime? sourceAddressReleaseMoment,
            DateTime? broadcastedTransactionForgetMoment,
            Guid operationId,
            string blockchainType,
            string fromAddress,
            string fromAddressContext,
            string toAddress,
            string assetId,
            decimal amount,
            bool includeFee,
            string transactionContext,
            string blockchainAssetId,
            string signedTransaction,
            string transactionHash,
            decimal? fee,
            string transactionError,
            long? transactionBlock,
            bool wasBroadcasted)
        {
            Version = version;
            State = state;
            Result = result;
            StartMoment = startMoment;
            TransactionBuildingMoment = transactionBuildingMoment;
            TransactionSigningMoment = transactionSigningMoment;
            TransactionBroadcastingMoment = transactionBroadcastingMoment;
            TransactionFinishMoment = transactionFinishMoment;
            SourceAddressReleaseMoment = sourceAddressReleaseMoment;
            BroadcastedTransactionForgetMoment = broadcastedTransactionForgetMoment;
            OperationId = operationId;
            BlockchainType = blockchainType;
            FromAddress = fromAddress;
            FromAddressContext = fromAddressContext;
            ToAddress = toAddress;
            AssetId = assetId;
            Amount = amount;
            IncludeFee = includeFee;
            TransactionContext = transactionContext;
            BlockchainAssetId = blockchainAssetId;
            SignedTransaction = signedTransaction;
            TransactionHash = transactionHash;
            Fee = fee;
            TransactionError = transactionError;
            TransactionBlock = transactionBlock;
            WasBroadcasted = wasBroadcasted;
        }

        public static OperationExecutionAggregate CreateNew(
            Guid operationId,
            string fromAddress,
            string toAddress,
            string blockchainType,
            string blockchainAssetId,
            string assetId,
            decimal amount,
            bool includeFee)
        {
            return new OperationExecutionAggregate(
                operationId,
                fromAddress,
                toAddress,
                blockchainType,
                blockchainAssetId,
                assetId,
                amount,
                includeFee);
        }

        public static OperationExecutionAggregate Restore(
            string version,
            OperationExecutionState state,
            OperationExecutionResult result,
            DateTime startMoment,
            DateTime? transactionBuildingMoment,
            DateTime? transactionSigningMoment,
            DateTime? transactionBroadcastingMoment,
            DateTime? transactionFinishMoment,
            DateTime? sourceAddressReleaseMoment,
            DateTime? broadcastedTransactionForgetMoment,
            Guid operationId,
            string blockchainType,
            string fromAddress,
            string fromAddressContext,
            string toAddress,
            string assetId,
            decimal amount,
            bool includeFee,
            string transactionContext,
            string blockchainAssetId,
            string signedTransaction,
            string transactionHash,
            decimal? fee,
            string transactionError,
            long? transactionBlock,
            bool wasBroadcasted)
        {
            return new OperationExecutionAggregate(
                version,
                state,
                result,
                startMoment,
                transactionBuildingMoment,
                transactionSigningMoment,
                transactionBroadcastingMoment,
                transactionFinishMoment,
                sourceAddressReleaseMoment,
                broadcastedTransactionForgetMoment,
                operationId,
                blockchainType,
                fromAddress,
                fromAddressContext,
                toAddress,
                assetId,
                amount,
                includeFee,
                transactionContext,
                blockchainAssetId,
                signedTransaction,
                transactionHash,
                fee,
                transactionError,
                transactionBlock,
                wasBroadcasted);
        }
        
        public bool OnTransactionBuilt(string fromAddressContext, string transactionContext, string blockchainType, string blockchainAssetId)
        {
            FromAddressContext = fromAddressContext;
            TransactionContext = transactionContext;
            BlockchainType = blockchainType;
            BlockchainAssetId = blockchainAssetId;

            TransactionBuildingMoment = DateTime.UtcNow;

            return true;
        }

        public bool OnTransactionRebuildingRequested()
        {
            return true;
        }

        public bool OnTransactionBuildingRejected()
        {
            // If transaction building is rejected, then address lock has been captured.
            // This is a redundant operation execution thread and main operation execution thread is probably
            // went futher.
            // So we should return true, if lock should be already released. We don't need
            // to switch state since this is just redundant operation execution thread.

            // Lock should be released right after broadcasting
            return !WasBroadcasted;
        }

        public bool OnTransactionReBuildingIsRequestedOnBroadcasting()
        {
            return true;
        }

        public bool OnTransactionBroadcastingFailed()
        {
            return true;
        }

        public bool OnTransactionBuildingFailed()
        {
            return true;
        }

        public bool OnTransactionSigned(string signedTransaction)
        {
            SignedTransaction = signedTransaction;

            TransactionSigningMoment = DateTime.UtcNow;

            return true;
        }

        public bool OnTransactionBroadcasted()
        {
            TransactionBroadcastingMoment = DateTime.UtcNow;
            WasBroadcasted = true;

            return true;
        }

        public bool OnSourceAddressLockReleased()
        {
            SourceAddressReleaseMoment = DateTime.UtcNow;

            return true;
        }

        public bool OnTransactionCompleted(string transactionHash, long transactionBlock, decimal fee)
        {
            TransactionHash = transactionHash;
            TransactionBlock = transactionBlock;
            Fee = fee;

            Result = OperationExecutionResult.Success;

            TransactionFinishMoment = DateTime.UtcNow;

            return true;
        }

        public bool OnTransactionFailed(string error)
        {
            TransactionError = error;

            Result = OperationExecutionResult.Failure;

            TransactionFinishMoment = DateTime.UtcNow;

            return true;
        }
        
        public bool OnBroadcastedTransactionForgotten()
        {
            BroadcastedTransactionForgetMoment = DateTime.UtcNow;

            return true;
        }
    }
}

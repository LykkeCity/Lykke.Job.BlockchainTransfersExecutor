using System;

namespace Lykke.Job.BlockchainOperationsExecutor.Core.Domain
{
    public class OperationExecutionAggregate
    {
        public string Version { get; }

        public OperationExecutionState State { get; private set; }
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
        public string BlockchainType { get; private set; }
        public string TransactionContext { get; private set; }
        public string BlockchainAssetId { get; private set; }
        public string SignedTransaction { get; private set; }
        public string TransactionHash { get; private set; }
        public decimal? Fee { get; private set; }
        public string TransactionError { get; private set; }
        
        private OperationExecutionAggregate(
            Guid operationId, 
            string fromAddress, 
            string toAddress, 
            string assetId, 
            decimal amount, 
            bool includeFee)
        {
            StartMoment = DateTime.UtcNow;

            OperationId = operationId;
            FromAddress = fromAddress;
            ToAddress = toAddress;
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
            string toAddress,
            string assetId,
            decimal amount,
            bool includeFee,
            string transactionContext,
            string blockchainAssetId,
            string signedTransaction,
            string transactionHash,
            decimal? fee,
            string transactionError)
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
        }

        public static OperationExecutionAggregate CreateNew(
            Guid operationId,
            string fromAddress,
            string toAddress,
            string assetId,
            decimal amount,
            bool includeFee)
        {
            return new OperationExecutionAggregate(
                operationId,
                fromAddress,
                toAddress,
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
            string toAddress,
            string assetId,
            decimal amount,
            bool includeFee,
            string transactionContext,
            string blockchainAssetId,
            string signedTransaction,
            string transactionHash,
            decimal? fee,
            string transactionError)
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
                toAddress,
                assetId,
                amount,
                includeFee,
                transactionContext,
                blockchainAssetId,
                signedTransaction,
                transactionHash,
                fee,
                transactionError);
        }
        
        public bool OnTransactionBuilt(string transactionContext, string blockchainType, string blockchainAssetId)
        {
            if (State != OperationExecutionState.Started)
            {
                return false;
            }

            TransactionContext = transactionContext;
            BlockchainType = blockchainType;
            BlockchainAssetId = blockchainAssetId;

            State = OperationExecutionState.TransactionIsBuilt;

            TransactionBuildingMoment = DateTime.UtcNow;

            return true;
        }

        public bool OnTransactionSigned(string signedTransaction)
        {
            if (State != OperationExecutionState.TransactionIsBuilt)
            {
                return false;
            }

            SignedTransaction = signedTransaction;

            State = OperationExecutionState.TransactionIsSigned;

            TransactionSigningMoment = DateTime.UtcNow;

            return true;
        }

        public bool OnTransactionBroadcasted()
        {
            if (State != OperationExecutionState.TransactionIsSigned)
            {
                return false;
            }

            State = OperationExecutionState.TransactionIsBroadcasted;

            TransactionBroadcastingMoment = DateTime.UtcNow;

            return true;
        }

        public bool OnTransactionCompleted(string transactionHash, decimal fee)
        {
            if (State != OperationExecutionState.TransactionIsBroadcasted)
            {
                return false;
            }

            TransactionHash = transactionHash;
            Fee = fee;

            State = OperationExecutionState.TransactionIsFinished;
            Result = OperationExecutionResult.Success;

            TransactionFinishMoment = DateTime.UtcNow;

            return true;
        }

        public bool OnTransactionFailed(string error)
        {
            if (State != OperationExecutionState.TransactionIsBroadcasted)
            {
                return false;
            }

            TransactionError = error;

            State = OperationExecutionState.TransactionIsFinished;
            Result = OperationExecutionResult.Failure;

            TransactionFinishMoment = DateTime.UtcNow;

            return true;
        }
        
        public bool OnSourceAddressLockReleased()
        {
            if (State != OperationExecutionState.TransactionIsFinished)
            {
                return false;
            }

            State = OperationExecutionState.SourceAddresIsReleased;

            SourceAddressReleaseMoment = DateTime.UtcNow;

            return true;
        }

        public bool OnBroadcastedTransactionForgotten()
        {
            if (State != OperationExecutionState.SourceAddresIsReleased)
            {
                return false;
            }

            State = OperationExecutionState.BroadcastedTransactionIsForgotten;

            BroadcastedTransactionForgetMoment = DateTime.UtcNow;

            return true;
        }
    }
}

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
        public string FromAddressContext { get; private set; }

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
                transactionError);
        }
        
        public bool OnTransactionBuilt(string addressContext, string transactionContext, string blockchainType, string blockchainAssetId)
        {
            if (!SwitchState(OperationExecutionState.Started, OperationExecutionState.TransactionIsBuilt))
            {
                return false;
            }

            FromAddressContext = addressContext;
            TransactionContext = transactionContext;
            BlockchainType = blockchainType;
            BlockchainAssetId = blockchainAssetId;

            TransactionBuildingMoment = DateTime.UtcNow;

            return true;
        }

        public bool OnTransactionSigned(string signedTransaction)
        {
            if (!SwitchState(OperationExecutionState.TransactionIsBuilt, OperationExecutionState.TransactionIsSigned))
            {
                return false;
            }
            
            SignedTransaction = signedTransaction;

            TransactionSigningMoment = DateTime.UtcNow;

            return true;
        }

        public bool OnTransactionBroadcasted()
        {
            if (!SwitchState(OperationExecutionState.TransactionIsSigned, OperationExecutionState.TransactionIsBroadcasted))
            {
                return false;
            }

            TransactionBroadcastingMoment = DateTime.UtcNow;

            return true;
        }

        public bool OnSourceAddressLockReleased()
        {
            if (!SwitchState(OperationExecutionState.TransactionIsBroadcasted, OperationExecutionState.SourceAddresIsReleased))
            {
                return false;
            }

            SourceAddressReleaseMoment = DateTime.UtcNow;

            return true;
        }

        public bool OnTransactionCompleted(string transactionHash, decimal fee)
        {
            if (!SwitchState(OperationExecutionState.SourceAddresIsReleased, OperationExecutionState.TransactionIsFinished))
            {
                return false;
            }

            TransactionHash = transactionHash;
            Fee = fee;

            Result = OperationExecutionResult.Success;

            TransactionFinishMoment = DateTime.UtcNow;

            return true;
        }

        public bool OnTransactionFailed(string error)
        {
            if (!SwitchState(OperationExecutionState.SourceAddresIsReleased, OperationExecutionState.TransactionIsFinished))
            {
                return false;
            }

            TransactionError = error;

            Result = OperationExecutionResult.Failure;

            TransactionFinishMoment = DateTime.UtcNow;

            return true;
        }
        
        public bool OnBroadcastedTransactionForgotten()
        {
            if (!SwitchState(OperationExecutionState.TransactionIsFinished, OperationExecutionState.BroadcastedTransactionIsForgotten))
            {
                return false;
            }

            BroadcastedTransactionForgetMoment = DateTime.UtcNow;

            return true;
        }

        private bool SwitchState(OperationExecutionState expectedState, OperationExecutionState nextState)
        {
            if (State < expectedState)
            {
                // Throws to retry and wait until aggregate will be in the required state
                throw new InvalidAggregateStateException(State, expectedState, nextState);
            }

            if (State > expectedState)
            {
                // Aggregate already in the next state, so this event can be just ignored
                return false;
            }

            State = nextState;

            return true;
        }
    }
}

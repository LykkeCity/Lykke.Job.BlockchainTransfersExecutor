using System;
using Lykke.Job.BlockchainOperationsExecutor.Contract;

namespace Lykke.Job.BlockchainOperationsExecutor.Core.Domain
{
    public class OperationExecutionAggregate
    {
        public string Version { get; }

        public OperationExecutionState State { get; private set; }

        public DateTime StartMoment { get; }
        public DateTime? TransactionBuildingMoment { get; private set; }
        public DateTime? TransactionSigningMoment { get; private set; }
        public DateTime? TransactionBroadcastingMoment { get; private set; }
        public DateTime? TransactionFinishMoment { get; private set; }
        public DateTime? SourceAddressReleaseMoment { get; private set; }

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
        }

        private OperationExecutionAggregate(
            string version, 
            OperationExecutionState state,
            DateTime startMoment,
            DateTime? transactionBuildingMoment,
            DateTime? transactionSigningMoment,
            DateTime? transactionBroadcastingMoment,
            DateTime? transactionFinishMoment,
            DateTime? sourceAddressReleaseMoment,
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
            StartMoment = startMoment;
            TransactionBuildingMoment = transactionBuildingMoment;
            TransactionSigningMoment = transactionSigningMoment;
            TransactionBroadcastingMoment = transactionBroadcastingMoment;
            TransactionFinishMoment = transactionFinishMoment;
            SourceAddressReleaseMoment = sourceAddressReleaseMoment;
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
            DateTime startMoment,
            DateTime? transactionBuildingMoment,
            DateTime? transactionSigningMoment,
            DateTime? transactionBroadcastingMoment,
            DateTime? transactionFinishMoment,
            DateTime? sourceAddressReleaseMoment,
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
                startMoment,
                transactionBuildingMoment,
                transactionSigningMoment,
                transactionBroadcastingMoment,
                transactionFinishMoment,
                sourceAddressReleaseMoment,
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

            State = OperationExecutionState.TransactionIsCompleted;

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

            State = OperationExecutionState.TransactionIsFailed;

            TransactionFinishMoment = DateTime.UtcNow;

            return true;
        }
        
        public bool OnSourceAddressLockReleased()
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (State)
            {
                case OperationExecutionState.TransactionIsCompleted:
                    State = OperationExecutionState.Completed;
                    break;

                case OperationExecutionState.TransactionIsFailed:
                    State = OperationExecutionState.Failed;
                    break;

                default:
                    return false;
            }

            SourceAddressReleaseMoment = DateTime.UtcNow;

            return true;
        }
    }
}

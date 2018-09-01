using System;
using System.Collections.Generic;

namespace Lykke.Job.BlockchainOperationsExecutor.Core.Domain.TransactionExecutions
{
    public class TransactionExecutionAggregate
    {
        public string Version { get; }

        public TransactionExecutionState State { get; private set; }
        public TransactionExecutionResult? Result { get; private set; }

        public DateTime StartMoment { get; }
        public DateTime? SourceAddressLockingMoment { get; private set; }
        public DateTime? BuildingMoment { get; private set; }
        public DateTime? SigningMoment { get; private set; }
        public DateTime? BroadcastingMoment { get; private set; }
        public DateTime? FinishMoment { get; private set; }
        public DateTime? SourceAddressReleasingMoment { get; private set; }
        public DateTime? ClearingMoment { get; private set; }
        public DateTime? BuildingFailureMoment { get; private set; }
        public DateTime? BroadcastinFailureMoment { get; private set; }
        public DateTime? WaitingForEndingStartMoment { get; private set; }
        public DateTime? WaitingForEndingFailureMoment { get; private set; }

        public Guid OperationId { get; }
        public Guid TransactionId { get; }
        public int TransactionNumber { get; }
        public string FromAddress { get; }
        public IReadOnlyCollection<TransactionOutputValueType> Outputs { get; }
        public string AssetId { get; }
        public bool IncludeFee { get; }

        public string BlockchainType { get; }
        public string BlockchainAssetId { get; }

        public string Context { get; private set; }
        public string SignedTransaction { get; private set; }
        public decimal? TransactionAmount { get; private set; }
        public long? Block { get; private set; }
        public decimal? Fee { get; private set; }
        public string Hash { get; private set; }
        public string Error { get; private set; }
        public string FromAddressContext { get; private set; }
        
        private TransactionExecutionAggregate(
            string version,
            Guid operationId, 
            Guid transactionId,
            int transactionNumber,
            string fromAddress, 
            IReadOnlyCollection<TransactionOutputValueType> outputs,
            string blockchainType,
            string blockchainAssetId,
            string assetId, 
            bool includeFee,
            TransactionExecutionState state,
            DateTime startMoment)
        {
            Version = version;
            OperationId = operationId;
            TransactionId = transactionId;
            TransactionNumber = transactionNumber;
            FromAddress = fromAddress;
            Outputs = outputs;
            BlockchainType = blockchainType;
            BlockchainAssetId = blockchainAssetId;
            AssetId = assetId;
            IncludeFee = includeFee;
            State = state;
            StartMoment = startMoment;
        }

        public static TransactionExecutionAggregate Start(
            Guid operationId,
            Guid transactionId,
            int transactionNumber,
            string fromAddress,
            IReadOnlyCollection<TransactionOutputValueType> outputs,
            string blockchainType,
            string blockchainAssetId,
            string assetId,
            bool includeFee)
        {
            return new TransactionExecutionAggregate(
                "*",
                operationId,
                transactionId,
                transactionNumber,
                fromAddress,
                outputs,
                blockchainType,
                blockchainAssetId,
                assetId,
                includeFee,
                TransactionExecutionState.Started,
                DateTime.UtcNow);
        }

        public static TransactionExecutionAggregate Restore(
            string version,
            TransactionExecutionState state,
            TransactionExecutionResult? result,
            DateTime startMoment,
            DateTime? sourceAddressLockingMoment,
            DateTime? buildingMoment,
            DateTime? signingMoment,
            DateTime? broadcastingMoment,
            DateTime? finishMoment,
            DateTime? sourceAddressReleasingMoment,
            DateTime? clearingMoment,
            DateTime? buildingFailureMoment,
            DateTime? broadcastinFailureMoment,
            DateTime? waitingForEndingStartMoment,
            DateTime? waitingForEndingFailureMoment,
            Guid operationId,
            Guid transactionId,
            int transactionNumber,
            string blockchainType,
            string fromAddress,
            string fromAddressContext,
            IReadOnlyCollection<TransactionOutputValueType> outputs,
            string assetId,
            bool includeFee,
            string context,
            string blockchainAssetId,
            string signedTransaction,
            decimal? transactionAmount,
            long? block,
            decimal? fee,
            string hash,
            string error)
        {
            return new TransactionExecutionAggregate(
                version,
                operationId,
                transactionId,
                transactionNumber,
                fromAddress,
                outputs,
                blockchainType,
                blockchainAssetId,
                assetId,
                includeFee,
                state,
                startMoment)
            {
                Result = result,
                SourceAddressLockingMoment = sourceAddressLockingMoment,
                BuildingMoment = buildingMoment,
                SigningMoment = signingMoment,
                BroadcastingMoment = broadcastingMoment,
                FinishMoment = finishMoment,
                SourceAddressReleasingMoment = sourceAddressReleasingMoment,
                ClearingMoment = clearingMoment,
                BuildingFailureMoment = buildingFailureMoment,
                BroadcastinFailureMoment = broadcastinFailureMoment,
                WaitingForEndingStartMoment = waitingForEndingStartMoment,
                WaitingForEndingFailureMoment = waitingForEndingFailureMoment,
                FromAddressContext = fromAddressContext,
                Context = context,
                SignedTransaction = signedTransaction,
                TransactionAmount = transactionAmount,
                Block = block,
                Fee = fee,
                Hash = hash,
                Error = error
            };
        }

        public void OnSourceAddressLocked()
        {
            State = TransactionExecutionState.SourceAddressLocked;

            SourceAddressLockingMoment = DateTime.UtcNow;
        }
        
        public void OnBuilt(string fromAddressContext, string transactionContext)
        {
            State = TransactionExecutionState.Built;

            BuildingMoment = DateTime.UtcNow;

            FromAddressContext = fromAddressContext;
            Context = transactionContext;
        }

        public void OnSigned(string signedTransaction)
        {
            State = TransactionExecutionState.Signed;

            SigningMoment = DateTime.UtcNow;

            SignedTransaction = signedTransaction;
        }

        public void OnBroadcasted()
        {
            State = TransactionExecutionState.Broadcasted;

            BroadcastingMoment = DateTime.UtcNow;
        }

        public void OnSourceAddressLockReleased()
        {
            State = TransactionExecutionState.SourceAddressReleased;

            SourceAddressReleasingMoment = DateTime.UtcNow;
        }

        public void OnCompleted(decimal amount, long block, decimal fee, string hash)
        {
            State = TransactionExecutionState.Completed;

            FinishMoment = DateTime.UtcNow;

            Result = TransactionExecutionResult.Completed;
            TransactionAmount = amount;
            Block = block;
            Fee = fee;
            Hash = hash;
        }
        
        public void OnCleared()
        {
            ClearingMoment = DateTime.UtcNow;

            State = TransactionExecutionState.Cleared;
        }

        public void OnBuildingFailed(TransactionExecutionResult errorCode, string error)
        {
            if (errorCode == TransactionExecutionResult.Completed)
            {
                throw new ArgumentException($"Error code should not be {TransactionExecutionResult.Completed}", nameof(errorCode));
            }

            State = TransactionExecutionState.BuildingFailed;

            BuildingFailureMoment = DateTime.UtcNow;

            Result = errorCode;
            Error = error;
        }

        public void OnBroadcastingFailed(TransactionExecutionResult errorCode, string error)
        {
            if (errorCode == TransactionExecutionResult.Completed)
            {
                throw new ArgumentException($"Error code should not be {TransactionExecutionResult.Completed}", nameof(errorCode));
            }

            State = TransactionExecutionState.BroadcastingFailed;

            BroadcastinFailureMoment = DateTime.UtcNow;

            Result = errorCode;
            Error = error;
        }

        public void OnWaitingForEndingStarted()
        {
            State = TransactionExecutionState.WaitingForEnding;

            WaitingForEndingStartMoment = DateTime.UtcNow;
        }

        public void OnWaitingForEndingFailed(TransactionExecutionResult errorCode, string error)
        {
            if (errorCode == TransactionExecutionResult.Completed)
            {
                throw new ArgumentException($"Error code should not be {TransactionExecutionResult.Completed}", nameof(errorCode));
            }

            State = TransactionExecutionState.WaitingForEndingFailed;

            WaitingForEndingFailureMoment = DateTime.UtcNow;

            Result = errorCode;
            Error = error;
        }
    }
}

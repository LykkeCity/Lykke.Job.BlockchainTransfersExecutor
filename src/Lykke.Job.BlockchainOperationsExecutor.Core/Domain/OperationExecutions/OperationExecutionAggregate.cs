using System;

namespace Lykke.Job.BlockchainOperationsExecutor.Core.Domain.OperationExecutions
{
    public class OperationExecutionAggregate
    {
        public string Version { get; }

        public OperationExecutionState State { get; private set; }
        public OperationExecutionResult? Result { get; private set; }

        public DateTime StartMoment { get; }
        public DateTime? ActiveTransactionIdGenerationMoment { get; private set; }
        public DateTime? ActiveTransactionStartMoment { get; private set; }
        public DateTime? TransactionExecutionRepeatRequestMoment { get; private set; }
        public DateTime? ActiveTransactionClearingMoment { get; private set; }
        public DateTime? TransactionFinishMoment { get; private set; }
        public DateTime? FinishMoment { get; private set; }

        public Guid OperationId { get; }

        public string FromAddress { get; }
        public string ToAddress { get; }
        public string AssetId { get; }
        public decimal Amount { get; }
        public bool IncludeFee { get; }
        public string BlockchainType { get; }
        public string BlockchainAssetId { get; }

        public Guid? ActiveTransactionId { get; private set; }
        public int ActiveTransactionNumber { get; private set; }
        public decimal TransactionAmount { get; private set; }
        public long TransactionBlock { get; private set; }
        public decimal TransactionFee { get; private set; }
        public string TransactionHash { get; private set; }
        public string Error { get; private set; }

        private OperationExecutionAggregate(
            string version,
            DateTime startMoment,
            Guid operationId,
            string fromAddress,
            string toAddress,
            string assetId,
            decimal amount,
            bool includeFee,
            string blockchainType,
            string blockchainAssetId)
        {
            Version = version;
            StartMoment = startMoment;
            OperationId = operationId;
            FromAddress = fromAddress;
            ToAddress = toAddress;
            AssetId = assetId;
            Amount = amount;
            IncludeFee = includeFee;
            BlockchainType = blockchainType;
            BlockchainAssetId = blockchainAssetId;
        }

        public static OperationExecutionAggregate Start(
            Guid operationId,
            string fromAddress,
            string toAddress,
            string assetId,
            decimal amount,
            bool includeFee,
            string blockchainType,
            string blockchainAssetId)
        {
            return new OperationExecutionAggregate(
                "*",
                DateTime.UtcNow,
                operationId,
                fromAddress,
                toAddress,
                assetId,
                amount,
                includeFee,
                blockchainType,
                blockchainAssetId)
            {
                State = OperationExecutionState.Started
            };
        }

        public static OperationExecutionAggregate Restore(
            string version,
            OperationExecutionState state,
            OperationExecutionResult? result,
            DateTime startMoment,
            DateTime? activeTransactionIdGenerationMoment,
            DateTime? activeTransactionExecutionStartMoment,
            DateTime? transactionExecutionRepetitionRequestMoment,
            DateTime? activeTransactionClearingMoment,
            DateTime? transactionFinishMoment,
            DateTime? finishMoment,
            Guid operationId,
            string fromAddress,
            string toAddress,
            string assetId,
            decimal amount,
            bool includeFee,
            string blockchainType,
            string blockchainAssetId,
            Guid? activeTransactionId,
            int activeTransactionNumber,
            decimal transactionAmount,
            long transactionBlock,
            decimal transactionFee,
            string transactionHash,
            string error)
        {
            return new OperationExecutionAggregate(
                version,
                startMoment,
                operationId,
                fromAddress,
                toAddress,
                assetId,
                amount,
                includeFee,
                blockchainType,
                blockchainAssetId)
            {
                State = state,
                Result = result,
                ActiveTransactionIdGenerationMoment = activeTransactionIdGenerationMoment,
                ActiveTransactionStartMoment = activeTransactionExecutionStartMoment,
                TransactionFinishMoment = transactionFinishMoment,
                TransactionExecutionRepeatRequestMoment = transactionExecutionRepetitionRequestMoment,
                ActiveTransactionClearingMoment = activeTransactionClearingMoment,
                FinishMoment = finishMoment,
                ActiveTransactionId = activeTransactionId,
                ActiveTransactionNumber = activeTransactionNumber,
                TransactionAmount = transactionAmount,
                TransactionBlock = transactionBlock,
                TransactionFee = transactionFee,
                TransactionHash = transactionHash,
                Error = error
            };
        }

        public void OnActiveTransactionIdGenerated(Guid transactionId, int transactionNumber)
        {
            State = OperationExecutionState.ActiveTransactionIdGenerated;

            ActiveTransactionIdGenerationMoment = DateTime.UtcNow;

            ActiveTransactionId = transactionId;
            ActiveTransactionNumber = transactionNumber;
        }

        public void OnTransactionExecutionStarted()
        {
            State = OperationExecutionState.TransactionExecutionInProgress;

            ActiveTransactionStartMoment = DateTime.UtcNow;
        }

        public void OnTransactionExecutionRepeatRequested(string error)
        {
            State = OperationExecutionState.TransactionExecutionRepeatRequested;

            TransactionExecutionRepeatRequestMoment = DateTime.UtcNow;

            Error = error;
        }

        public void OnActiveTransactionCleared()
        {
            State = OperationExecutionState.ActiveTransactionCleared;

            ActiveTransactionClearingMoment = DateTime.UtcNow;

            ActiveTransactionId = null;
        }

        public void OnTransactionExecutionCompleted(
            decimal transactionAmount, 
            long transactionBlock, 
            decimal transactionFee, 
            string transactionHash)
        {
            State = OperationExecutionState.Completed;

            TransactionFinishMoment = DateTime.UtcNow;

            Result = OperationExecutionResult.Completed;
            TransactionAmount = transactionAmount;
            TransactionBlock = transactionBlock;
            TransactionFee = transactionFee;
            TransactionHash = transactionHash;
        }

        public void OnTransactionExecutionFailed(int transactionNumber, OperationExecutionResult errorCode, string error)
        {
            State = OperationExecutionState.Failed;

            TransactionFinishMoment = DateTime.UtcNow;

            Result = errorCode;
            Error = error;
        }

        public void OnNotifiedAboutEnding()
        {
            State = OperationExecutionState.NotifiedAboutEnding;

            FinishMoment = DateTime.UtcNow;
        }
    }
}

using Lykke.Job.BlockchainOperationsExecutor.Contract.Errors;
using Lykke.Service.BlockchainApi.Contract;

namespace Lykke.Job.BlockchainOperationsExecutor.Helpers
{
    public static class ErrorCodeMappers
    {
        public static OperationExecutionErrorCode MapToOperationExecutionErrorCode(
            this TransactionBroadcastingErrorCode source)
        {
            switch (source)
            {
                case TransactionBroadcastingErrorCode.AmountTooSmall:
                    return OperationExecutionErrorCode.AmountTooSmall;
                default:
                    return OperationExecutionErrorCode.Unknown;
            }
        }

        public static OperationExecutionErrorCode MapToOperationExecutionErrorCode(
            this TransactionBuildingErrorCode source)
        {
            switch (source)
            {
                case TransactionBuildingErrorCode.AmountTooSmall:
                    return OperationExecutionErrorCode.AmountTooSmall;
                default:
                    return OperationExecutionErrorCode.Unknown;
            }
        }

        public static OperationExecutionErrorCode MapToOperationExecutionErrorCode(
            this BlockchainErrorCode source)
        {
            switch (source)
            {
                case BlockchainErrorCode.AmountIsTooSmall:
                    return OperationExecutionErrorCode.AmountTooSmall;
                default:
                    return OperationExecutionErrorCode.Unknown;
            }
        }
    }
}

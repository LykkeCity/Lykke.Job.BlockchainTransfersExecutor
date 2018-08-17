using System;
using Lykke.Job.BlockchainOperationsExecutor.Contract;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.OperationExecutions;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.TransactionExecutions;
using Lykke.Service.BlockchainApi.Contract;

namespace Lykke.Job.BlockchainOperationsExecutor.Mappers
{
    public static class MappingExtensions
    {
        public static TransactionExecutionResult MapToTransactionExecutionResult(
            this BlockchainErrorCode source)
        {
            switch (source)
            {
                case BlockchainErrorCode.AmountIsTooSmall:
                    return TransactionExecutionResult.AmountIsTooSmall;

                case BlockchainErrorCode.NotEnoughBalance:
                    return TransactionExecutionResult.NotEnoughBalance;

                case BlockchainErrorCode.Unknown:
                    return TransactionExecutionResult.UnknownError;

                case BlockchainErrorCode.BuildingShouldBeRepeated:
                    return TransactionExecutionResult.RebuildingIsRequired;

                default:
                    throw new ArgumentOutOfRangeException(nameof(source), source, null);
            }
        }

        public static OperationExecutionResult MapToOperationExecutionResult(this TransactionExecutionResult source)
        {
            switch (source)
            {
                case TransactionExecutionResult.Completed:
                    return OperationExecutionResult.Completed;
                    
                case TransactionExecutionResult.UnknownError:
                    return OperationExecutionResult.UnknownError;

                case TransactionExecutionResult.AmountIsTooSmall:
                    return OperationExecutionResult.AmountIsTooSmall;

                default:
                    throw new ArgumentOutOfRangeException(nameof(source), source, null);
            }
        }

        public static OperationExecutionErrorCode MapToOperationExecutionErrorCode(this OperationExecutionResult source)
        {
            switch (source)
            {
                case OperationExecutionResult.UnknownError:
                    return OperationExecutionErrorCode.Unknown;

                case OperationExecutionResult.AmountIsTooSmall:
                    return OperationExecutionErrorCode.AmountTooSmall;

                default:
                    throw new ArgumentOutOfRangeException(nameof(source), source, null);
            }
        }
    }
}

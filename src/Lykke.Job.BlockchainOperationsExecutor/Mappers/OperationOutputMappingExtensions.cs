using Lykke.Job.BlockchainOperationsExecutor.Contract;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.TransactionExecutions;

namespace Lykke.Job.BlockchainOperationsExecutor.Mappers
{
    public static class OperationOutputMappingExtensions
    {
        public static OperationOutput ToContract(this TransactionOutputValueType source)
        {
            return new OperationOutput
            {
                Address = source.Address,
                Amount = source.Amount
            };
        }
        public static TransactionOutputValueType FromContract(this OperationOutput source)
        {
            return new TransactionOutputValueType(source.Address, source.Amount);
        }
    }
}
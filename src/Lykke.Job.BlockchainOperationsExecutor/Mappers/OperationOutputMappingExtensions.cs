using Lykke.Job.BlockchainOperationsExecutor.Contract;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.TransactionExecutions;

namespace Lykke.Job.BlockchainOperationsExecutor.Mappers
{
    public static class OperationOutputMappingExtensions
    {
        public static OperationEndpoint ToContract(this TransactionEndpointValueType source)
        {
            return new OperationEndpoint
            {
                Address = source.Address,
                Amount = source.Amount
            };
        }
        public static TransactionEndpointValueType FromContract(this OperationEndpoint source)
        {
            return new TransactionEndpointValueType(source.Address, source.Amount);
        }
    }
}
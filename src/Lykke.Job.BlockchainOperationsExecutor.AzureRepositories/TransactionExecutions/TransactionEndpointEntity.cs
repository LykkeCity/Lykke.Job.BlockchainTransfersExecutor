using JetBrains.Annotations;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.TransactionExecutions;

namespace Lykke.Job.BlockchainOperationsExecutor.AzureRepositories.TransactionExecutions
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    internal class TransactionEndpointEntity
    {
        public string Address { get; set; }
        public decimal Amount { get; set; }
        public TransactionEndpointValueType ToDomain()
        {
            return new TransactionEndpointValueType(Address, Amount);
        }
        public static TransactionEndpointEntity FromDomain(TransactionEndpointValueType source)
        {
            return new TransactionEndpointEntity
            {
                Amount = source.Amount,
                Address = source.Address
            };
        }
    }
}

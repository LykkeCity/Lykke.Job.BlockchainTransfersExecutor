using JetBrains.Annotations;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.TransactionExecutions;

namespace Lykke.Job.BlockchainOperationsExecutor.AzureRepositories.TransactionExecutions
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    internal class TransactionOutputEntity
    {
        public string Address { get; set; }
        public decimal Amount { get; set; }
        
        public static TransactionOutputEntity FromDomain(TransactionOutputValueType source)
        {
            return new TransactionOutputEntity
            {
                Amount = source.Amount,
                Address = source.Address
            };
        }

        public TransactionOutputValueType ToDomain()
        {
            return new TransactionOutputValueType(Address, Amount);
        }
    }
}

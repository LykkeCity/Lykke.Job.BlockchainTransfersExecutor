namespace Lykke.Job.BlockchainOperationsExecutor.Core.Domain.TransactionExecutions
{
    public class TransactionEndpointValueType
    {
        public string Address { get; }
            
        public decimal Amount { get; }

        public TransactionEndpointValueType(string address, decimal amount)
        {
            Address = address;
            Amount = amount;
        }
    }
}

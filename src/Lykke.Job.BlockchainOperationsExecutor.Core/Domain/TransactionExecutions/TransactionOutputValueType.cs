namespace Lykke.Job.BlockchainOperationsExecutor.Core.Domain.TransactionExecutions
{
    public class TransactionOutputValueType
    {
        public string Address { get; }
            
        public decimal Amount { get; }

        public TransactionOutputValueType(string address, decimal amount)
        {
            Address = address;
            Amount = amount;
        }
    }
}

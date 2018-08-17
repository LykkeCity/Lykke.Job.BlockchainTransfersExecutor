using System;
using Common;
using Lykke.AzureStorage.Tables;

namespace Lykke.Job.BlockchainOperationsExecutor.AzureRepositories.TransactionExecutions
{
    public class SourceAddressLockEntity : AzureTableEntity
    {
        public Guid OwnerTransactionId { get; set; }

        public static string GetPartitionKey(string blockchainType, string address)
        {
            // Use hash to distribute all records to the different partitions
            var hash = address.CalculateHexHash32(3);

            return $"{blockchainType}-{hash}";
        }

        public static string GetRowKey(string address)
        {
            return address;
        }
    }
}

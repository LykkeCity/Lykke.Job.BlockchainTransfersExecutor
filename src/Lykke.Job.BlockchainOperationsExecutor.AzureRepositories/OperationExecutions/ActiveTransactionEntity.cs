using System;
using Common;
using Lykke.AzureStorage.Tables;

namespace Lykke.Job.BlockchainOperationsExecutor.AzureRepositories.OperationExecutions
{
    internal class ActiveTransactionEntity : AzureTableEntity
    {
        public Guid TransactionId { get; set; }

        public static string GetPartitionKey(Guid operationId)
        {
            // Use hash to distribute all records to the different partitions
            var hash = operationId.ToString().CalculateHexHash32(3);

            return hash;
        }

        public static string GetRowKey(Guid operationId)
        {
            return $"{operationId:D}";
        }
    }
}
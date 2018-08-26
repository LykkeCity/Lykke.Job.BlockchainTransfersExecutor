using System;
using Common;

namespace Lykke.Job.BlockchainOperationsExecutor.AzureRepositories
{
    internal static class AggregateKeysBuilder
    {
        public static string BuildPartitionKey(Guid aggregateId)
        {
            // Use hash to distribute all records to the different partitions
            var hash = aggregateId.ToString().CalculateHexHash32(3);

            return hash;
        }

        public static string BuildRowKey(Guid aggregateId)
        {
            return $"{aggregateId:D}";
        }
    }
}

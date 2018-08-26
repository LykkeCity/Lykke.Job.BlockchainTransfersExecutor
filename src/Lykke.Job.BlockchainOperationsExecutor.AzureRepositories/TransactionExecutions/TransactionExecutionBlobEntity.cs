using System;
using JetBrains.Annotations;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.TransactionExecutions;

namespace Lykke.Job.BlockchainOperationsExecutor.AzureRepositories.TransactionExecutions
{
    internal class TransactionExecutionBlobEntity
    {
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public string TransactionContext { get; set; }
        
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public string SignedTransaction { get; set; }

        public static string GetContainerName(string blockchainType)
        {
            return $"transaction-executions-{blockchainType.ToLower()}";
        }

        public static string GetBlobName(Guid operationId)
        {
            return operationId.ToString();
        }

        public static TransactionExecutionBlobEntity FromDomain(TransactionExecutionAggregate aggregate)
        {
            return new TransactionExecutionBlobEntity
            {
                TransactionContext = aggregate.Context,
                SignedTransaction = aggregate.SignedTransaction
            };
        }
    }
}

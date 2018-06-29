using System;
using JetBrains.Annotations;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain;

namespace Lykke.Job.BlockchainOperationsExecutor.AzureRepositories
{
    internal class OperationExecutionBlobEntity
    {
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public string TransactionContext { get; set; }
        
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public string SignedTransaction { get; set; }

        public static string GetContainerName(string blockchainType)
        {
            return $"operation-executions-{blockchainType.ToLower()}";
        }

        public static string GetBlobName(Guid operationId)
        {
            return operationId.ToString();
        }

        public static OperationExecutionBlobEntity FromDomain(OperationExecutionAggregate aggregate)
        {
            return new OperationExecutionBlobEntity
            {
                TransactionContext = aggregate.TransactionContext,
                SignedTransaction = aggregate.SignedTransaction
            };
        }
    }
}

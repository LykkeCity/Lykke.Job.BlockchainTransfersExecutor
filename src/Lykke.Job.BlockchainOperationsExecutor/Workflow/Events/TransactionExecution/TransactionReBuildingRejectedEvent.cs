using System;
using JetBrains.Annotations;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.TransactionExecution
{
    /// <summary>
    /// Blockchain transaction  rebuilding is rejected
    /// </summary>
    [MessagePackObject(keyAsPropertyName: true)]
    public class TransactionReBuildingRejectedEvent
    {
        [UsedImplicitly(ImplicitUseKindFlags.Access)]
        public Guid OperationId { get; set; }
    }
}

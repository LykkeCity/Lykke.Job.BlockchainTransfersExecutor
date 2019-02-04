using System;
using System.Collections.Generic;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.TransactionExecution;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.CommandHandlers.TransactionExecution
{
    public static class CommandHandlerEventConfigurer
    {
        public static ICollection<(string eventKey, Type eventType)> ConfigureCapturedEvents()
        {
            return new[]
            {
                ("TransactionBuiltEvent", typeof(TransactionBuiltEvent)),
                ("TransactionExecutionFailedEvent", typeof(TransactionExecutionFailedEvent)),
                ("TransactionBuildingRejectedEvent", typeof(TransactionBuildingRejectedEvent)),
                ("TransactionSignedEvent", typeof(TransactionSignedEvent)),
            };
        }
    }
}

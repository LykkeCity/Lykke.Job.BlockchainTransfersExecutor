using System;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Contract.Events;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands.OperationExecution;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.CommandHandlers.OperationExecution
{
    [UsedImplicitly]
    public class NotifyOperationExecutionCompletedCommandsHandler
    {
        [UsedImplicitly]
        public Task<CommandHandlingResult> Handle(NotifyOperationExecutionCompletedCommand command, IEventPublisher publisher)
        {
            if (command.TransactionOutputs.Length > 1)
            {
                publisher.PublishEvent
                (
                    new OneToManyOperationExecutionCompletedEvent
                    {
                        OperationId = command.OperationId,
                        TransactionId = command.TransactionId,
                        TransactionHash = command.TransactionHash,
                        TransactionOutputs = command.TransactionOutputs,
                        Fee = command.TransactionFee,
                        Block = command.TransactionBlock
                    }
                );
            } 
            else if (command.TransactionOutputs.Length == 1)
            {
                publisher.PublishEvent
                (
                    new OperationExecutionCompletedEvent
                    {
                        OperationId = command.OperationId,
                        TransactionId = command.TransactionId,
                        TransactionHash = command.TransactionHash,
                        TransactionAmount = command.TransactionOutputs.Single().Amount,
                        Fee = command.TransactionFee,
                        Block = command.TransactionBlock
                    }
                );
            }
            else
            {
                throw new InvalidOperationException("There should be at least one output");
            }

            return Task.FromResult(CommandHandlingResult.Ok());
        }
    }
}

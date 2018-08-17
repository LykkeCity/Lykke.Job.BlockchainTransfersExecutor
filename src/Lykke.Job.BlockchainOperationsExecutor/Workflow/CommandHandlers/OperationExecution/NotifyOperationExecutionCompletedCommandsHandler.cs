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
            publisher.PublishEvent(new OperationExecutionCompletedEvent
            {
                OperationId = command.OperationId,
                TransactionHash = command.TransactionHash,
                TransactionAmount = command.TransactionAmount,
                Fee = command.TransactionFee,
                Block = command.TransactionBlock
            });

            return Task.FromResult(CommandHandlingResult.Ok());
        }
    }
}

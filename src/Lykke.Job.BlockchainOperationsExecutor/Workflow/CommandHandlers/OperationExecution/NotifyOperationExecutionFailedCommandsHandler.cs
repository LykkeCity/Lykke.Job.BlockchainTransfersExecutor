using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Contract.Events;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands.OperationExecution;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.CommandHandlers.OperationExecution
{
    [UsedImplicitly]
    public class NotifyOperationExecutionFailedCommandsHandler
    {
        [UsedImplicitly]
        public Task<CommandHandlingResult> Handle(NotifyOperationExecutionFailedCommand command, IEventPublisher publisher)
        {
            publisher.PublishEvent(new OperationExecutionFailedEvent
            {
                OperationId = command.OperationId,
                Error = command.Error,
                ErrorCode = command.ErrorCode
            });

            return Task.FromResult(CommandHandlingResult.Ok());
        }
    }
}

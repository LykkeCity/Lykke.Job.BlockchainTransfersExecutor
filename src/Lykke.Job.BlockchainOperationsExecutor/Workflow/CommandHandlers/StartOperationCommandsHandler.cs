using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Contract.Commands;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Events;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.CommandHandlers
{
    [UsedImplicitly]
    public class StartOperationCommandsHandler
    {
        [UsedImplicitly]
        public Task<CommandHandlingResult> Handle(StartOperationCommand command, IEventPublisher publisher)
        {
            // TODO: In the further there could be a start of the operations aggregation.
            // Just by saving them to the storage for example.
            // Waiting period for aggregation should be passed in the command to let
            // the initiator of the command decide how long operation can wait to aggregate 
            // with other operations

            publisher.PublishEvent(new OperationStartRequestedEvent
            {
                OperationId = command.OperationId,
                FromAddress = command.FromAddress,
                ToAddress = command.ToAddress,
                AssetId = command.AssetId,
                Amount = command.Amount,
                IncludeFee = command.IncludeFee
            });

            return Task.FromResult(CommandHandlingResult.Ok());
        }
    }
}

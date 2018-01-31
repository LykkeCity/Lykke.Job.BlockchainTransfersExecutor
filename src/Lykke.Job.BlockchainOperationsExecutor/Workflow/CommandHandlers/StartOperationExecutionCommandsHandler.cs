using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Contract.Commands;
using Lykke.Job.BlockchainOperationsExecutor.Contract.Events;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.CommandHandlers
{
    [UsedImplicitly]
    public class StartOperationExecutionCommandsHandler
    {
        private readonly ILog _log;

        public StartOperationExecutionCommandsHandler(ILog log)
        {
            _log = log;
        }

        [UsedImplicitly]
        public Task<CommandHandlingResult> Handle(StartOperationExecutionCommand command, IEventPublisher publisher)
        {

            _log.WriteInfo(nameof(StartOperationExecutionCommand), command, "");

            // TODO: In the further there could be a start of the operations aggregation.
            // Just by saving them to the storage for example.
            // Waiting period for aggregation should be passed in the command to let
            // the initiator of the command decide how long operation can wait to aggregate 
            // with other operations

            publisher.PublishEvent(new OperationExecutionStartedEvent
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

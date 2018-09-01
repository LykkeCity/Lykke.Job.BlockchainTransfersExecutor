using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands.TransactionExecution;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.TransactionExecution;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.CommandHandlers.TransactionExecution
{
    [UsedImplicitly]
    public class StartTransactionExecutionCommandHandler
    {
        [UsedImplicitly]
        public Task<CommandHandlingResult> Handle(StartTransactionExecutionCommand command, IEventPublisher publisher)
        {
            publisher.PublishEvent(new TransactionExecutionStartedEvent
            {
                OperationId = command.OperationId,
                TransactionId = command.TransactionId,
                TransactionNumber = command.TransactionNumber,
                BlockchainType = command.BlockchainType,
                BlockchainAssetId = command.BlockchainAssetId,
                AssetId = command.AssetId,
                FromAddress = command.FromAddress,
                ToEndpoints = command.ToEndpoints,
                IncludeFee = command.IncludeFee
            });

            return Task.FromResult(CommandHandlingResult.Ok());
        }
    }
}

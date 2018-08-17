using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.OperationExecutions;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands.OperationExecution;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Events;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.CommandHandlers.OperationExecution
{
    public class ClearActiveTransactionCommandsHandler
    {
        private readonly IActiveTransactionsRepository _activeTransactionsRepository;

        public ClearActiveTransactionCommandsHandler(IActiveTransactionsRepository activeTransactionsRepository)
        {
            _activeTransactionsRepository = activeTransactionsRepository;
        }

        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(ClearActiveTransactionCommand command, IEventPublisher publisher)
        {
            await _activeTransactionsRepository.EndTransactionAsync(command.OperationId, command.TransactionId);

            publisher.PublishEvent(new ActiveTransactionClearedEvent
            {
                OperationId = command.OperationId
            });

            return CommandHandlingResult.Ok();
        }
    }
}

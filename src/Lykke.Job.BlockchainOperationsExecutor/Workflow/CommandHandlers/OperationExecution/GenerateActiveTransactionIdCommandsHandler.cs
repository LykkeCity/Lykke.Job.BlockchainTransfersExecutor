using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.OperationExecutions;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands.OperationExecution;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.OperationExecution;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.CommandHandlers.OperationExecution
{
    public class GenerateActiveTransactionIdCommandsHandler
    {
        private readonly IActiveTransactionsRepository _activeTransactionsRepository;

        public GenerateActiveTransactionIdCommandsHandler(IActiveTransactionsRepository activeTransactionsRepository)
        {
            _activeTransactionsRepository = activeTransactionsRepository;
        }

        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(GenerateActiveTransactionIdCommand command, IEventPublisher publisher)
        {
            var activeTransactionId = await _activeTransactionsRepository.GetOrStartTransactionAsync(
                command.OperationId,
                Guid.NewGuid);

            publisher.PublishEvent(new ActiveTransactionIdGeneratedEvent
            {
                OperationId = command.OperationId,
                TransactionId = activeTransactionId
            });

            return CommandHandlingResult.Ok();
        }
    }
}

using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.TransactionExecutions;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands.TransactionExecution;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.TransactionExecution;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.CommandHandlers.TransactionExecution
{
    [UsedImplicitly]
    public class ReleaseSourceAddressLockCommandsHandler
    {
        private readonly IChaosKitty _chaosKitty;
        private readonly IAddressLocksRepository _locksRepository;

        public ReleaseSourceAddressLockCommandsHandler(
            IChaosKitty chaosKitty,
            IAddressLocksRepository locksRepository)
        {
            _chaosKitty = chaosKitty;
            _locksRepository = locksRepository;
        }

        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(ReleaseSourceAddressLockCommand command, IEventPublisher publisher)
        {
            await _locksRepository.ReleaseOutputExclusiveLockAsync
            (
                command.BlockchainType,
                command.FromAddress
            );

            _chaosKitty.Meow(command.TransactionId);

            if (!command.AbortWorkflow)
            {
                publisher.PublishEvent(new SourceAddressLockReleasedEvent
                {
                    OperationId = command.OperationId,
                    TransactionId = command.TransactionId
                });
            }

            return CommandHandlingResult.Ok();
        }
    }
}

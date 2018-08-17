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
        private readonly ISourceAddresLocksRepoistory _locksRepoistory;

        public ReleaseSourceAddressLockCommandsHandler(
            IChaosKitty chaosKitty,
            ISourceAddresLocksRepoistory locksRepoistory)
        {
            _chaosKitty = chaosKitty;
            _locksRepoistory = locksRepoistory;
        }

        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(ReleaseSourceAddressLockCommand command, IEventPublisher publisher)
        {
            await _locksRepoistory.ReleaseLockAsync(command.BlockchainType, command.FromAddress, command.TransactionId);

            _chaosKitty.Meow(command.TransactionId);

            publisher.PublishEvent(new SourceAddressLockReleasedEvent
            {
                TransactionId = command.TransactionId
            });

            return CommandHandlingResult.Ok();
        }
    }
}

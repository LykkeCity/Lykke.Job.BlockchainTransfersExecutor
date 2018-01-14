using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Core;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Events;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.CommandHandlers
{
    [UsedImplicitly]
    public class ReleaseSourceAddressLockCommandsHandler
    {
        private readonly ISourceAddresLocksRepoistory _locksRepoistory;

        public ReleaseSourceAddressLockCommandsHandler(ISourceAddresLocksRepoistory locksRepoistory)
        {
            _locksRepoistory = locksRepoistory;
        }

        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(ReleaseSourceAddressLockCommand command, IEventPublisher publisher)
        {
            await _locksRepoistory.ReleaseLockAsync(command.BlockchainType, command.FromAddress, command.OperationId);

            ChaosKitty.Meow();

            publisher.PublishEvent(new SourceAddressLockReleasedEvent
            {
                OperationId = command.OperationId
            });

            ChaosKitty.Meow();

            return CommandHandlingResult.Ok();
        }
    }
}

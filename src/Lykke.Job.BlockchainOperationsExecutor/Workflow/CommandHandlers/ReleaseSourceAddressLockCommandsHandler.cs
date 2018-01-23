using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Contract.Events;
using Lykke.Job.BlockchainOperationsExecutor.Core;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.CommandHandlers
{
    [UsedImplicitly]
    public class ReleaseSourceAddressLockCommandsHandler
    {
        private readonly ILog _log;
        private readonly ISourceAddresLocksRepoistory _locksRepoistory;

        public ReleaseSourceAddressLockCommandsHandler(
            ILog log,
            ISourceAddresLocksRepoistory locksRepoistory)
        {
            _log = log;
            _locksRepoistory = locksRepoistory;
        }

        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(ReleaseSourceAddressLockCommand command, IEventPublisher publisher)
        {
#if DEBUG
            _log.WriteInfo(nameof(ReleaseSourceAddressLockCommand), command, "");
#endif
            await _locksRepoistory.ReleaseLockAsync(command.BlockchainType, command.FromAddress, command.OperationId);

            ChaosKitty.Meow(command.OperationId);

            publisher.PublishEvent(new SourceAddressLockReleasedEvent
            {
                OperationId = command.OperationId
            });

            return CommandHandlingResult.Ok();
        }
    }
}

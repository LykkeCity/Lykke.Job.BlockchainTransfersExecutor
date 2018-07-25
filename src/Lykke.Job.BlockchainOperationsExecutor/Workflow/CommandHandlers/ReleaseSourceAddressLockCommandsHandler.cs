using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Contract.Events;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain;
using Lykke.Job.BlockchainOperationsExecutor.Core.Services.Blockchains;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.CommandHandlers
{
    [UsedImplicitly]
    public class ReleaseSourceAddressLockCommandsHandler
    {
        private readonly IChaosKitty _chaosKitty;
        private readonly ISourceAddresLocksRepoistory _locksRepoistory;
        private readonly ICapabilitiesService _capabilitiesService;

        public ReleaseSourceAddressLockCommandsHandler(
            IChaosKitty chaosKitty,
            ISourceAddresLocksRepoistory locksRepoistory,
            ICapabilitiesService capabilitiesService)
        {
            _chaosKitty = chaosKitty;
            _locksRepoistory = locksRepoistory;
            _capabilitiesService = capabilitiesService;
        }

        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(ReleaseSourceAddressLockCommand command, IEventPublisher publisher)
        {
            var capabilities = await _capabilitiesService.GetAsync(command.BlockchainType);
            if (!capabilities.IsExclusiveWithdrawalsRequired)
            {
                await _locksRepoistory.ReleaseLockAsync(command.BlockchainType, command.FromAddress, command.OperationId);

                _chaosKitty.Meow(command.OperationId);
            }

            publisher.PublishEvent(new SourceAddressLockReleasedEvent
            {
                OperationId = command.OperationId
            });

            return CommandHandlingResult.Ok();
        }
    }
}

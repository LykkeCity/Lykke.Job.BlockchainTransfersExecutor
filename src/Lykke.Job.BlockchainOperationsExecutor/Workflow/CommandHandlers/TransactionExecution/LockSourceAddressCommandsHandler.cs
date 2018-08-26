using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.TransactionExecutions;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands.TransactionExecution;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.TransactionExecution;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.CommandHandlers.TransactionExecution
{
    public class LockSourceAddressCommandsHandler
    {
        private readonly IChaosKitty _chaosKitty;
        private readonly ISourceAddresLocksRepoistory _sourceAddresLocksRepoistory;
        private readonly RetryDelayProvider _retryDelayProvider;

        public LockSourceAddressCommandsHandler(
            IChaosKitty chaosKitty, 
            ISourceAddresLocksRepoistory sourceAddresLocksRepoistory, 
            RetryDelayProvider retryDelayProvider)
        {
            _chaosKitty = chaosKitty;
            _sourceAddresLocksRepoistory = sourceAddresLocksRepoistory;
            _retryDelayProvider = retryDelayProvider;
        }

        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(LockSourceAddressCommand command, IEventPublisher publisher)
        {
            var isSourceAdressCaptured = await _sourceAddresLocksRepoistory.TryGetLockAsync(
                command.BlockchainType,
                command.FromAddress,
                command.TransactionId);

            if (!isSourceAdressCaptured)
            {
                return CommandHandlingResult.Fail(_retryDelayProvider.SourceAddressLockingRetryDelay);
            }

            _chaosKitty.Meow(command.TransactionId);

            publisher.PublishEvent(new SourceAddressLockedEvent
            {
                OperationId = command.OperationId,
                TransactionId = command.TransactionId
            });

            return CommandHandlingResult.Ok();
        }
    }
}

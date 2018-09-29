using System.Threading.Tasks;
using Common.Log;
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
        private readonly ILog _log;
        private readonly IChaosKitty _chaosKitty;
        private readonly ITransactionExecutionsRepository _transactionExecutionsRepository;
        private readonly ISourceAddresLocksRepoistory _sourceAddresLocksRepoistory;
        private readonly RetryDelayProvider _retryDelayProvider;

        public LockSourceAddressCommandsHandler(
            ILog log, 
            IChaosKitty chaosKitty, 
            ITransactionExecutionsRepository transactionExecutionsRepository,
            ISourceAddresLocksRepoistory sourceAddresLocksRepoistory, 
            RetryDelayProvider retryDelayProvider)
        {
            _log = log.CreateComponentScope(nameof(LockSourceAddressCommandsHandler));
            _chaosKitty = chaosKitty;
            _transactionExecutionsRepository = transactionExecutionsRepository;
            _sourceAddresLocksRepoistory = sourceAddresLocksRepoistory;
            _retryDelayProvider = retryDelayProvider;
        }

        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(LockSourceAddressCommand command, IEventPublisher publisher)
        {
            var transactionExecution = await _transactionExecutionsRepository.GetAsync(command.TransactionId);

            if (transactionExecution.WasLocked)
            {
                _log.WriteInfo(nameof(LockSourceAddressCommand), command, "Source address lock command has been skipped, since lock already was performed earlier");

                return CommandHandlingResult.Ok();
            }

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

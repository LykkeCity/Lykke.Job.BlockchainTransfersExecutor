using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Common.Log;
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
        private readonly IAddressLocksRepository _addressLocksRepository;
        private readonly RetryDelayProvider _retryDelayProvider;

        public LockSourceAddressCommandsHandler(
            ILogFactory logFactory,
            IChaosKitty chaosKitty,
            ITransactionExecutionsRepository transactionExecutionsRepository,
            IAddressLocksRepository addressLocksRepository, 
            RetryDelayProvider retryDelayProvider)
        {
            _log = logFactory.CreateLog(this);
            _chaosKitty = chaosKitty;
            _transactionExecutionsRepository = transactionExecutionsRepository;
            _addressLocksRepository = addressLocksRepository;
            _retryDelayProvider = retryDelayProvider;
        }

        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(LockSourceAddressCommand command, IEventPublisher publisher)
        {
            var transactionExecution = await _transactionExecutionsRepository.GetAsync(command.TransactionId);

            if (transactionExecution.WasLocked)
            {
                _log.Info("Source address lock command has been skipped, since lock already was performed earlier", command);

                return CommandHandlingResult.Ok();
            }

            var sourceAddressLocked = await _addressLocksRepository.TryExclusivelyLockOutputAsync
            (
                command.BlockchainType,
                command.FromAddress,
                command.TransactionId
            );

            if (sourceAddressLocked)
            {
                _chaosKitty.Meow(command.TransactionId);

                publisher.PublishEvent(new SourceAddressLockedEvent
                {
                    OperationId = command.OperationId,
                    TransactionId = command.TransactionId
                });

                return CommandHandlingResult.Ok();
            }
            else
            {
                return CommandHandlingResult.Fail(_retryDelayProvider.SourceAddressLockingRetryDelay);
            }
        }
    }
}

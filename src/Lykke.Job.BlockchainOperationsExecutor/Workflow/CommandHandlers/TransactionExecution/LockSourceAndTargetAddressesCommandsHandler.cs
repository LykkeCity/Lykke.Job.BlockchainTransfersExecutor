using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.TransactionExecutions;
using Lykke.Job.BlockchainOperationsExecutor.Core.Services.Blockchains;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands.TransactionExecution;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.TransactionExecution;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.CommandHandlers.TransactionExecution
{
    public class LockSourceAndTargetAddressesCommandsHandler
    {
        private readonly ILog _log;
        private readonly ITransactionExecutionsRepository _transactionExecutionsRepository;
        private readonly IAddressLocksRepository _addressLocksRepository;
        private readonly RetryDelayProvider _retryDelayProvider;
        private readonly IBlockchainSettingsProvider _blockchainSettingsProvider;

        public LockSourceAndTargetAddressesCommandsHandler(
            ILogFactory logFactory,
            ITransactionExecutionsRepository transactionExecutionsRepository,
            IAddressLocksRepository addressLocksRepository, 
            RetryDelayProvider retryDelayProvider,
            IBlockchainSettingsProvider blockchainSettingsProvider)
        {
            _log = logFactory.CreateLog(this);
            _transactionExecutionsRepository = transactionExecutionsRepository;
            _addressLocksRepository = addressLocksRepository;
            _retryDelayProvider = retryDelayProvider;
            _blockchainSettingsProvider = blockchainSettingsProvider;
        }
        
        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(LockSourceAndTargetAddressesCommand command, IEventPublisher publisher)
        {
            var transactionExecution = await _transactionExecutionsRepository.GetAsync(command.TransactionId);

            if (transactionExecution.WasLocked)
            {
                _log.Info("Source and target addresses lock command has been skipped, since lock already was performed earlier", command);

                return CommandHandlingResult.Ok();
            }

            #region Locks shortcuts

            Task ConcurrentlyLockInputAsync(string address)
            {
                return _addressLocksRepository.ConcurrentlyLockInputAsync
                (
                    command.BlockchainType,
                    address,
                    command.OperationId
                );
            }
            
            Task ConcurrentlyLockOutputAsync(string address)
            {
                return _addressLocksRepository.ConcurrentlyLockOutputAsync
                (
                    command.BlockchainType,
                    address,
                    command.OperationId
                );
            }
            
            Task<bool> TryExclusivelyLockInputAsync(string address)
            {
                return _addressLocksRepository.TryExclusivelyLockInputAsync
                (
                    command.BlockchainType,
                    address,
                    command.OperationId
                );
            }
            
            Task<bool> TryExclusivelyLockOutputAsync(string address)
            {
                return _addressLocksRepository.TryExclusivelyLockOutputAsync
                (
                    command.BlockchainType,
                    address,
                    command.OperationId
                );
            }

            Task<bool> IsInputInExclusiveLockAsync(string address)
            {
                return _addressLocksRepository.IsInputInExclusiveLockAsync
                (
                    command.BlockchainType,
                    address
                );
            }
            
            #endregion
            
            var from = command.FromAddress;
            var to = command.ToAddress;
            var hwAddress = _blockchainSettingsProvider.GetHotWalletAddress(command.BlockchainType);
            var retryLater = CommandHandlingResult.Fail(_retryDelayProvider.SourceAddressLockingRetryDelay);

            if (from == hwAddress)
            {
                if (!await TryExclusivelyLockOutputAsync(from))
                {
                    return retryLater;
                }

                if (!await TryExclusivelyLockInputAsync(from))
                {
                    return retryLater;
                }
            }

            if (to == hwAddress)
            {
                if (!await TryExclusivelyLockOutputAsync(from))
                {
                    return retryLater;
                }

                await ConcurrentlyLockOutputAsync(to);
                await ConcurrentlyLockInputAsync(to);

                if (await IsInputInExclusiveLockAsync(to))
                {
                    return retryLater;
                }
            }
            
            publisher.PublishEvent(new SourceAndTargetAddressesLockedEvent
            {
                OperationId = command.OperationId,
                TransactionId = command.TransactionId
            });
            
            return CommandHandlingResult.Ok();
        }
    }
}

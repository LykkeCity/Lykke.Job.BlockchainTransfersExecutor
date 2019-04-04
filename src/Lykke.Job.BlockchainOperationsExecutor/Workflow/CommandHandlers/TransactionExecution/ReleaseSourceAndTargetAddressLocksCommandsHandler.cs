using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.TransactionExecutions;
using Lykke.Job.BlockchainOperationsExecutor.Core.Services.Blockchains;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands.TransactionExecution;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.TransactionExecution;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.CommandHandlers.TransactionExecution
{
    [UsedImplicitly]
    public class ReleaseSourceAndTargetAddressLocksCommandsHandler
    {
        private readonly IAddressLocksRepository _locksRepository;
        private readonly IBlockchainSettingsProvider _blockchainSettingsProvider;

        public ReleaseSourceAndTargetAddressLocksCommandsHandler(
            IAddressLocksRepository locksRepository,
            IBlockchainSettingsProvider blockchainSettingsProvider)
        {
            _locksRepository = locksRepository;
            _blockchainSettingsProvider = blockchainSettingsProvider;
        }

        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(ReleaseSourceAndTargetAddressLocksCommand command, IEventPublisher publisher)
        {
            #region Locks shortcuts

            Task ReleaseInputConcurrentLockAsync(string address)
            {
                return _locksRepository.ReleaseInputConcurrentLockAsync
                (
                    command.BlockchainType,
                    address,
                    command.TransactionId
                );
            }

            Task ReleaseInputExclusiveLockAsync(string address)
            {
                return _locksRepository.ReleaseInputExclusiveLockAsync
                (
                    command.BlockchainType,
                    address,
                    command.TransactionId
                );
            }

            Task ReleaseOutputConcurrentLockAsync(string address)
            {
                return _locksRepository.ReleaseOutputConcurrentLockAsync
                (
                    command.BlockchainType,
                    address,
                    command.TransactionId
                );
            }

            Task ReleaseOutputExclusiveLockAsync(string address)
            {
                return _locksRepository.ReleaseOutputExclusiveLockAsync
                (
                    command.BlockchainType,
                    address,
                    command.TransactionId
                );
            }
            
            #endregion
            
            var from = command.FromAddress;
            var to = command.ToAddress;
            var hwAddress = _blockchainSettingsProvider.GetHotWalletAddress(command.BlockchainType);

            if (from == hwAddress)
            {
                await ReleaseInputExclusiveLockAsync(from);
                await ReleaseOutputExclusiveLockAsync(from);
            }

            if (to == hwAddress)
            {
                await ReleaseInputConcurrentLockAsync(to);
                await ReleaseOutputConcurrentLockAsync(to);
                await ReleaseOutputExclusiveLockAsync(from);
            }

            if (!command.AbortWorkflow)
            {
                publisher.PublishEvent(new SourceAndTargetAddressLocksReleasedEvent
                {
                    OperationId = command.OperationId,
                    TransactionId = command.TransactionId
                });
            }
            
            return CommandHandlingResult.Ok();
        }
    }
}

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
    public class ForgetBroadcastedTransactionCommandsHandler
    {
        private readonly IChaosKitty _chaosKitty;
        private readonly IBlockchainApiClientProvider _apiClientProvider;
        private readonly ICapabilitiesService _capabilitiesService;
        private readonly ISourceAddresLocksRepoistory _locksRepoistory;

        public ForgetBroadcastedTransactionCommandsHandler(
            IChaosKitty chaosKitty,
            IBlockchainApiClientProvider apiClientProvider,
            ICapabilitiesService capabilitiesService,
            ISourceAddresLocksRepoistory locksRepoistory)
        {
            _chaosKitty = chaosKitty;
            _apiClientProvider = apiClientProvider;
            _capabilitiesService = capabilitiesService;
            _locksRepoistory = locksRepoistory;
        }

        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(ForgetBroadcastedTransactionCommand command, IEventPublisher publisher)
        {
            var apiClient = _apiClientProvider.Get(command.BlockchainType);

            var capabilities = await _capabilitiesService.GetAsync(command.BlockchainType);
            if (capabilities.IsExclusiveWithdrawalsRequired)
            {
                await _locksRepoistory.ReleaseLockAsync(command.BlockchainType, command.FromAddress, command.OperationId);
                _chaosKitty.Meow(command.OperationId);

                await _locksRepoistory.ReleaseLockAsync(command.BlockchainType, command.ToAddress, command.OperationId);
                _chaosKitty.Meow(command.OperationId);
            }

            await apiClient.ForgetBroadcastedTransactionsAsync(command.OperationId);

            _chaosKitty.Meow(command.OperationId);

            publisher.PublishEvent(new BroadcastedTransactionForgottenEvent
            {
                OperationId = command.OperationId
            });

            return CommandHandlingResult.Ok();
        }
    }
}

using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Contract.Events;
using Lykke.Job.BlockchainOperationsExecutor.Core.Services.Blockchains;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.CommandHandlers
{
    [UsedImplicitly]
    public class ForgetBroadcastedTransactionCommandsHandler
    {
        private readonly IChaosKitty _chaosKitty;
        private readonly IBlockchainApiClientProvider _apiClientProvider;

        public ForgetBroadcastedTransactionCommandsHandler(
            IChaosKitty chaosKitty,
            IBlockchainApiClientProvider apiClientProvider)
        {
            _chaosKitty = chaosKitty;
            _apiClientProvider = apiClientProvider;
        }

        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(ForgetBroadcastedTransactionCommand command, IEventPublisher publisher)
        {
            var apiClient = _apiClientProvider.Get(command.BlockchainType);

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

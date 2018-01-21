using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Contract.Events;
using Lykke.Job.BlockchainOperationsExecutor.Core;
using Lykke.Job.BlockchainOperationsExecutor.Core.Services.Blockchains;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.CommandHandlers
{
    [UsedImplicitly]
    public class ForgetBroadcastedTransactionCommandsHandler
    {
        private readonly IBlockchainApiClientProvider _apiClientProvider;

        public ForgetBroadcastedTransactionCommandsHandler(IBlockchainApiClientProvider apiClientProvider)
        {
            _apiClientProvider = apiClientProvider;
        }

        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(ForgetBroadcastedTransactionCommand command, IEventPublisher publisher)
        {
            var apiClient = _apiClientProvider.Get(command.BlockchainType);

            await apiClient.ForgetBroadcastedTransactionsAsync(command.OperationId);

            ChaosKitty.Meow();

            publisher.PublishEvent(new BroadcastedTransactionForgottenEvent
            {
                OperationId = command.OperationId
            });

            return CommandHandlingResult.Ok();
        }
    }
}

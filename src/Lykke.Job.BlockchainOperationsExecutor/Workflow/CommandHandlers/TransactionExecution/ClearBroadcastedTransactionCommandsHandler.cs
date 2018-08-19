using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Core.Services.Blockchains;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands.TransactionExecution;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.TransactionExecution;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.CommandHandlers.TransactionExecution
{
    [UsedImplicitly]
    public class ClearBroadcastedTransactionCommandsHandler
    {
        private readonly IChaosKitty _chaosKitty;
        private readonly IBlockchainApiClientProvider _apiClientProvider;

        public ClearBroadcastedTransactionCommandsHandler(
            IChaosKitty chaosKitty,
            IBlockchainApiClientProvider apiClientProvider)
        {
            _chaosKitty = chaosKitty;
            _apiClientProvider = apiClientProvider;
        }

        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(ClearBroadcastedTransactionCommand command, IEventPublisher publisher)
        {
            var apiClient = _apiClientProvider.Get(command.BlockchainType);

            await apiClient.ForgetBroadcastedTransactionsAsync(command.TransactionId);

            _chaosKitty.Meow(command.TransactionId);

            publisher.PublishEvent(new BroadcastedTransactionClearedEvent
            {
                OperationId = command.OperationId,
                TransactionId = command.TransactionId
            });

            return CommandHandlingResult.Ok();
        }
    }
}

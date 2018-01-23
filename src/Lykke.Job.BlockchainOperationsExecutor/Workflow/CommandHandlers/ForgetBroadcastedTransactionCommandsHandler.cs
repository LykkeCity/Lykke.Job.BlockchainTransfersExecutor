using System.Threading.Tasks;
using Common.Log;
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
        private readonly ILog _log;
        private readonly IBlockchainApiClientProvider _apiClientProvider;

        public ForgetBroadcastedTransactionCommandsHandler(
            ILog log,
            IBlockchainApiClientProvider apiClientProvider)
        {
            _log = log;
            _apiClientProvider = apiClientProvider;
        }

        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(ForgetBroadcastedTransactionCommand command, IEventPublisher publisher)
        {
#if DEBUG
            _log.WriteInfo(nameof(ForgetBroadcastedTransactionCommand), command, "");
#endif
            var apiClient = _apiClientProvider.Get(command.BlockchainType);

            await apiClient.ForgetBroadcastedTransactionsAsync(command.OperationId);

            ChaosKitty.Meow(command.OperationId);

            publisher.PublishEvent(new BroadcastedTransactionForgottenEvent
            {
                OperationId = command.OperationId
            });

            return CommandHandlingResult.Ok();
        }
    }
}

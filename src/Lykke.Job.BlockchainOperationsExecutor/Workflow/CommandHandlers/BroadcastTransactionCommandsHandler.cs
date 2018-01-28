using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Contract.Events;
using Lykke.Job.BlockchainOperationsExecutor.Core.Services.Blockchains;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.CommandHandlers
{
    [UsedImplicitly]
    public class BroadcastTransactionCommandsHandler
    {
        private readonly IChaosKitty _chaosKitty;
        private readonly ILog _log;
        private readonly IBlockchainApiClientProvider _apiClientProvider;

        public BroadcastTransactionCommandsHandler(
            IChaosKitty chaosKitty,
            ILog log,
            IBlockchainApiClientProvider apiClientProvider)
        {
            _chaosKitty = chaosKitty;
            _log = log;
            _apiClientProvider = apiClientProvider;
        }

        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(BroadcastTransactionCommand command, IEventPublisher publisher)
        {
#if DEBUG
            _log.WriteInfo(nameof(BroadcastTransactionCommand), command, "");
#endif
            var apiClient = _apiClientProvider.Get(command.BlockchainType);

            if (!await apiClient.BroadcastTransactionAsync(command.OperationId, command.SignedTransaction))
            {
                _log.WriteInfo(nameof(BroadcastTransactionCommand), command.OperationId,
                    "API said that transaction is already broadcasted");
            }

            _chaosKitty.Meow(command.OperationId);


            publisher.PublishEvent(new TransactionBroadcastedEvent
            {
                OperationId = command.OperationId
            });

            return CommandHandlingResult.Ok();
        }
    }
}

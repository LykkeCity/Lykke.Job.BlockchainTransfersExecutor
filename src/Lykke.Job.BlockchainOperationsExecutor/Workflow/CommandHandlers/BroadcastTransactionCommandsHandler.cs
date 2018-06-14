using System;
using System.Threading.Tasks;
using System.Transactions;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Contract.Events;
using Lykke.Job.BlockchainOperationsExecutor.Core.Services.Blockchains;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands;
using Lykke.Service.BlockchainApi.Client.Models;

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
            var apiClient = _apiClientProvider.Get(command.BlockchainType);
            var broadcastingResult = await apiClient.BroadcastTransactionAsync(command.OperationId, command.SignedTransaction);

            switch (broadcastingResult)
            {
                case TransactionBroadcastingResult.Success:
                    break;
                
                case TransactionBroadcastingResult.AlreadyBroadcasted:
                    await _log.WriteInfoAsync
                    (
                        nameof(BroadcastTransactionCommandsHandler),
                        nameof(BroadcastTransactionCommand),
                        command.ToString(),
                        "API said that transaction is already broadcasted"
                    );
                    break;

                case TransactionBroadcastingResult.AmountIsTooSmall:
                case TransactionBroadcastingResult.NotEnoughBalance:
                    throw new TransactionException
                    (
                        $"Failed to broadcast transaction: {broadcastingResult}."
                    );
                
                default:
                    throw new ArgumentOutOfRangeException
                    (
                        nameof(broadcastingResult),
                        $"Transaction broadcastring result [{broadcastingResult}] is not supported."
                    );
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

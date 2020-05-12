using System;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Controllers;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.TransactionExecutions;
using Lykke.Job.BlockchainOperationsExecutor.Core.Services.Blockchains;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands.TransactionExecution;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.TransactionExecution;
using Lykke.Service.BlockchainApi.Client.Models;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.CommandHandlers.TransactionExecution
{
    [UsedImplicitly]
    public class BroadcastTransactionCommandsHandler
    {
        private readonly IChaosKitty _chaosKitty;
        private readonly ILog _log;
        private readonly IBlockchainApiClientProvider _apiClientProvider;
        private readonly RetryDelayProvider _retryDelayProvider;

        public BroadcastTransactionCommandsHandler(
            IChaosKitty chaosKitty,
            ILogFactory logFactory,
            IBlockchainApiClientProvider apiClientProvider, 
            RetryDelayProvider retryDelayProvider)
        {
            _chaosKitty = chaosKitty;
            _log = logFactory.CreateLog(this);
            _apiClientProvider = apiClientProvider;
            _retryDelayProvider = retryDelayProvider;
        }

        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(BroadcastTransactionCommand command, IEventPublisher publisher)
        {
            var apiClient = _apiClientProvider.Get(command.BlockchainType);
            var broadcastingResult = await apiClient.BroadcastTransactionAsync(command.TransactionId, command.SignedTransaction);

            _chaosKitty.Meow(command.TransactionId);

            switch (broadcastingResult)
            {
                case TransactionBroadcastingResult.Success:
                    
                    publisher.PublishEvent(new TransactionBroadcastedEvent
                    {
                        OperationId = command.OperationId,
                        TransactionId = command.TransactionId
                    });

                    return CommandHandlingResult.Ok();
                
                case TransactionBroadcastingResult.AlreadyBroadcasted:

                    _log.Info("API said that the transaction already was broadcasted", command);

                    publisher.PublishEvent(new TransactionBroadcastedEvent
                    {
                        OperationId = command.OperationId,
                        TransactionId = command.TransactionId
                    });
                    
                    return CommandHandlingResult.Ok();

                case TransactionBroadcastingResult.AmountIsTooSmall:
                    
                    _log.Warning("API said, that amount is too small", context: command);

                    publisher.PublishEvent(new TransactionExecutionFailedEvent
                    {
                        OperationId = command.OperationId,
                        TransactionId = command.TransactionId,
                        TransactionNumber = command.TransactionNumber,
                        ErrorCode = TransactionExecutionResult.AmountIsTooSmall,
                        Error = "Amount is to small"
                    });

                    return CommandHandlingResult.Ok();

                case TransactionBroadcastingResult.NotEnoughBalance:

                    _log.Info("API said, that balance is not enough to proceed the transaction", command);

                    return CommandHandlingResult.Fail(_retryDelayProvider.NotEnoughBalanceRetryDelay);

                case TransactionBroadcastingResult.BuildingShouldBeRepeated:

                    _log.Info("API said, that building should be repeated", command);

                    publisher.PublishEvent(new TransactionExecutionRepeatRequestedEvent
                    {
                        OperationId = command.OperationId,
                        TransactionId = command.TransactionId,
                        TransactionNumber = command.TransactionNumber,
                        ErrorCode = TransactionExecutionResult.RebuildingIsRequired,
                        Error = "Transaction building should be repeated"
                    });

                    return CommandHandlingResult.Ok();

                default:
                    throw new ArgumentOutOfRangeException
                    (
                        nameof(broadcastingResult),
                        $"Transaction broadcastring result [{broadcastingResult}] is not supported."
                    );
            }
        }
    }
}

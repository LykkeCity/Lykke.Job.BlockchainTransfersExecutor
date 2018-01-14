using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Contract.Events;
using Lykke.Job.BlockchainOperationsExecutor.Core;
using Lykke.Job.BlockchainOperationsExecutor.Core.Services.Blockchains;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands;
using Lykke.Service.BlockchainApi.Contract.Transactions;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.CommandHandlers
{
    [UsedImplicitly]
    public class WaitForTransactionEndingCommandsHandler
    {
        private readonly RetryDelayProvider _delayProvider;
        private readonly IBlockchainApiClientProvider _apiClientProvider;

        public WaitForTransactionEndingCommandsHandler(
            RetryDelayProvider delayProvider,
            IBlockchainApiClientProvider apiClientProvider)
        {
            _delayProvider = delayProvider;
            _apiClientProvider = apiClientProvider;
        }

        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(WaitForTransactionEndingCommand command, IEventPublisher publisher)
        {
            var apiClient = _apiClientProvider.Get(command.BlockchainType);

            // TODO: Cache it

            var blockchainAsset = await apiClient.GetAssetAsync(command.BlockchainAssetId);

            // TODO: Check for tranaction rebuild availability and publish TransactionTimeoutEvent after configured timeout
            // to run transaction rebuild process path

            var transaction = await apiClient.TryGetBroadcastedTransactionAsync(command.OperationId, blockchainAsset);

            if (transaction == null)
            {
                return CommandHandlingResult.Fail(_delayProvider.WaitForTransactionRetryDelay);
            }

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (transaction.State)
            {
                case BroadcastedTransactionState.Completed:
                    publisher.PublishEvent(new OperationCompletedEvent
                    {
                        OperationId = transaction.OperationId,
                        TransactionTimestamp = transaction.Timestamp,
                        TransactionHash = transaction.Hash,
                        TransactionAmount = transaction.Amount,
                        Fee = transaction.Fee
                    });

                    ChaosKitty.Meow();

                    await apiClient.ForgetBroadcastedTransactionsAsync(command.OperationId);
                    return CommandHandlingResult.Ok();

                case BroadcastedTransactionState.Failed:
                    publisher.PublishEvent(new OperationFailedEvent
                    {
                        OperationId = transaction.OperationId,
                        TransactionTimestamp = transaction.Timestamp,
                        Error = transaction.Error
                    });

                    ChaosKitty.Meow();

                    await apiClient.ForgetBroadcastedTransactionsAsync(command.OperationId);
                    return CommandHandlingResult.Ok();
            }

            return CommandHandlingResult.Fail(_delayProvider.WaitForTransactionRetryDelay);
        }
    }
}

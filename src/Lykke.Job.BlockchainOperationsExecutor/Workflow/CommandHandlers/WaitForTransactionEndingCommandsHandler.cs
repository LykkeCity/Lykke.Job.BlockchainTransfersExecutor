using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Contract.Events;
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
            if(command.OperationId == new System.Guid("ca8d2152-43a8-4731-993d-e9fe8495d8d2"))
            {
                publisher.PublishEvent(new OperationExecutionFailedEvent
                    {
                        OperationId = command.OperationId,
                        Error = "Aborted manually by kryazantsev. To workaround Bitshares API error due to too high tx expiration. We've faced with this issue on release"
                    });

                return CommandHandlingResult.Ok();
            }

            var apiClient = _apiClientProvider.Get(command.BlockchainType);

            // TODO: Cache it

            var blockchainAsset = await apiClient.GetAssetAsync(command.BlockchainAssetId);

            // TODO: Check for the availability of the tranaction rebuilding function and publish 
            // TransactionTimeoutEvent after configured timeout to run transaction rebuild process path

            var transaction = await apiClient.TryGetBroadcastedSingleTransactionAsync(command.OperationId, blockchainAsset);

            if (transaction == null)
            {
                // Transaction already has been forgotten, this means, 
                // that process has been went further and no events should be generated here.

                return CommandHandlingResult.Ok();
            }
            
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (transaction.State)
            {
                case BroadcastedTransactionState.Completed:
                    publisher.PublishEvent(new OperationExecutionCompletedEvent
                    {
                        OperationId = transaction.OperationId,
                        TransactionHash = transaction.Hash,
                        TransactionAmount = transaction.Amount,
                        Fee = transaction.Fee,
                        Block = transaction.Block
                    });

                    return CommandHandlingResult.Ok();

                case BroadcastedTransactionState.Failed:
                    publisher.PublishEvent(new OperationExecutionFailedEvent
                    {
                        OperationId = transaction.OperationId,
                        Error = transaction.Error
                    });

                    return CommandHandlingResult.Ok();
            }

            return CommandHandlingResult.Fail(_delayProvider.WaitForTransactionRetryDelay);
        }
    }
}

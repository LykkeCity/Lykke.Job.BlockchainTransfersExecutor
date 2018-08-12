using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Contract.Errors;
using Lykke.Job.BlockchainOperationsExecutor.Contract.Events;
using Lykke.Job.BlockchainOperationsExecutor.Core.Services.Blockchains;
using Lykke.Job.BlockchainOperationsExecutor.Helpers;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Events;
using Lykke.Service.BlockchainApi.Contract;
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
            //transaction building or broadcasting failure - so we do not have to check transaction state from blockchain api
            if (command.ErrorCode != null)
            {
                publisher.PublishEvent(new OperationExecutionFailedEvent
                {
                    OperationId = command.OperationId,
                    ErrorCode = command.ErrorCode.Value
                });
            }

            var apiClient = _apiClientProvider.Get(command.BlockchainType);

            // TODO: Cache it

            var blockchainAsset = await apiClient.GetAssetAsync(command.BlockchainAssetId);

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

                    if (transaction.ErrorCode == BlockchainErrorCode.NotEnoughBalance ||
                        transaction.ErrorCode == BlockchainErrorCode.BuildingShouldBeRepeated)
                    {
                        publisher.PublishEvent(new TransactionReBuildingIsRequestedEvent
                        {
                            OperationId = command.OperationId
                        });
                    }
                    else
                    {
                        publisher.PublishEvent(new OperationExecutionFailedEvent
                        {
                            OperationId = transaction.OperationId,
                            Error = transaction.Error,
                            ErrorCode = transaction.ErrorCode?.MapToOperationExecutionErrorCode() ?? OperationExecutionErrorCode.Unknown
                        });
                    }

                    return CommandHandlingResult.Ok();
            }

            return CommandHandlingResult.Fail(_delayProvider.WaitForTransactionRetryDelay);
        }
    }
}

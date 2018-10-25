using System;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.TransactionExecutions;
using Lykke.Job.BlockchainOperationsExecutor.Core.Services.Blockchains;
using Lykke.Job.BlockchainOperationsExecutor.Mappers;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands.TransactionExecution;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.TransactionExecution;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Transactions;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.CommandHandlers.TransactionExecution
{
    [UsedImplicitly]
    public class WaitForTransactionEndingCommandsHandler
    {
        private readonly ILog _log;
        private readonly RetryDelayProvider _delayProvider;
        private readonly IBlockchainApiClientProvider _apiClientProvider;

        public WaitForTransactionEndingCommandsHandler(
            ILogFactory logFactory,
            RetryDelayProvider delayProvider,
            IBlockchainApiClientProvider apiClientProvider)
        {
            _log = logFactory.CreateLog(this);
            _delayProvider = delayProvider;
            _apiClientProvider = apiClientProvider;
        }

        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(WaitForTransactionEndingCommand command, IEventPublisher publisher)
        {
            var apiClient = _apiClientProvider.Get(command.BlockchainType);

            // TODO: Cache it

            var blockchainAsset = await apiClient.GetAssetAsync(command.BlockchainAssetId);
            var transaction = await apiClient.TryGetBroadcastedSingleTransactionAsync(command.TransactionId, blockchainAsset);

            if (transaction == null)
            {
                _log.Info("Blockchain API returned no transaction. Assuming, that it's already was cleared", command);

                // Transaction already has been forgotten, this means, 
                // that process has been went further and no events should be generated here.

                return CommandHandlingResult.Ok();
            }
            
            switch (transaction.State)
            {
                case BroadcastedTransactionState.InProgress:
                    
                    return CommandHandlingResult.Fail(_delayProvider.WaitForTransactionRetryDelay);

                case BroadcastedTransactionState.Completed:

                    publisher.PublishEvent(new TransactionExecutionCompletedEvent
                    {
                        OperationId = command.OperationId,
                        TransactionId = command.TransactionId,
                        TransactionNumber = command.TransactionNumber,
                        TransactionHash = transaction.Hash,
                        TransactionAmount = transaction.Amount,
                        TransactionFee = transaction.Fee,
                        TransactionBlock = transaction.Block
                    });

                    return CommandHandlingResult.Ok();

                case BroadcastedTransactionState.Failed:

                    if (transaction.ErrorCode == BlockchainErrorCode.NotEnoughBalance ||
                        transaction.ErrorCode == BlockchainErrorCode.BuildingShouldBeRepeated ||
                        transaction.ErrorCode == BlockchainErrorCode.Unknown)
                    {
                        publisher.PublishEvent(new TransactionExecutionRepeatRequestedEvent
                        {
                            OperationId = command.OperationId,
                            TransactionId = command.TransactionId,
                            TransactionNumber = command.TransactionNumber,
                            ErrorCode = transaction.ErrorCode.Value.MapToTransactionExecutionResult(),
                            Error = transaction.Error
                        });
                    }
                    else
                    {
                        publisher.PublishEvent(new TransactionExecutionFailedEvent
                        {
                            OperationId = command.OperationId,
                            TransactionId = command.TransactionId,
                            TransactionNumber = command.TransactionNumber,
                            ErrorCode = transaction.ErrorCode?.MapToTransactionExecutionResult() ?? TransactionExecutionResult.UnknownError,
                            Error = transaction.Error
                        });
                    }

                    return CommandHandlingResult.Ok();

                default:
                    throw new ArgumentOutOfRangeException
                    (
                        nameof(transaction.State),
                        $"Transaction state [{transaction.State}] is not supported."
                    );
            }
        }
    }
}

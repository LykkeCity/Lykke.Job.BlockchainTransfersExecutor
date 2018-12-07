using System;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.OperationExecutions;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands.OperationExecution;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.OperationExecution;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.TransactionExecution;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.CommandHandlers.OperationExecution
{
    public class GenerateActiveTransactionIdCommandsHandler
    {
        private readonly IActiveTransactionsRepository _activeTransactionsRepository;
        private readonly IOperationExecutionsRepository _repository;
        private readonly RetryDelayProvider _retryDelayProvider;
        private readonly ILog _log;

        public GenerateActiveTransactionIdCommandsHandler(IActiveTransactionsRepository activeTransactionsRepository, 
            IOperationExecutionsRepository repository,
            RetryDelayProvider retryDelayProvider,
            ILogFactory logFactory)
        {
            _activeTransactionsRepository = activeTransactionsRepository;
            _repository = repository;
            _retryDelayProvider = retryDelayProvider;
            _log = logFactory.CreateLog(this);
        }

        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(GenerateActiveTransactionIdCommand command, IEventPublisher publisher)
        {
            if (command.IsCashout 
                && command.ActiveTransactioNumber > 0)  // generate active transaction id after rebuild command
            {
                var aggregate = await _repository.GetAsync(command.OperationId);

                var loggingContext = new
                {
                    aggregate.OperationId,
                    aggregateActiveTransactionNumber = aggregate.ActiveTransactionNumber,
                    commandActiveTransactionNumber = command.ActiveTransactioNumber
                };

                if (aggregate.ActiveTransactionNumber > command.ActiveTransactioNumber)
                {
                    _log.Warning("Command already handled. " +
                                 "Do nothing",
                        context: loggingContext);

                    return CommandHandlingResult.Ok();
                }

                if (aggregate.IsFinished)
                {
                    _log.Warning("Operation already finished. " +
                                 "Do nothing",
                        context: loggingContext);

                    return CommandHandlingResult.Ok();
                }


                switch (aggregate.RebuildConfirmationResult)
                {
                    case RebuildConfirmationResult.Unconfirmed:
                    {
                        _log.Warning("Transaction rebuild manual confirmation required. " +
                                     "RebuildConfirmationResult will checked later again",
                            context: loggingContext);

                        return CommandHandlingResult.Fail(_retryDelayProvider.RebuildingConfirmationCheckRetryDelay);
                    }
                    case RebuildConfirmationResult.Accepted:
                    {
                        _log.Warning("Transaction rebuild manual confirmation granted. " +
                                     "Transaction rebuilding started", 
                            context: loggingContext);

                        break;
                    }
                    case RebuildConfirmationResult.Rejected:
                    {
                        _log.Warning("Transaction rebuild manual confirmation rejected. " +
                                     "Transaction rebuilding rejected", 
                            context: loggingContext);


                        publisher.PublishEvent(new TransactionReBuildingRejectedEvent
                        {
                            OperationId = command.OperationId
                        });

                        return CommandHandlingResult.Ok();
                    }
                    default:
                    {
                        throw new ArgumentException($"Unknown switch {aggregate.RebuildConfirmationResult}. " +
                                                    "This should not happen",
                            nameof(aggregate.RebuildConfirmationResult));
                    }
                }
            }

            var activeTransactionId = await _activeTransactionsRepository.GetOrStartTransactionAsync(
                command.OperationId,
                Guid.NewGuid);

            publisher.PublishEvent(new ActiveTransactionIdGeneratedEvent
            {
                OperationId = command.OperationId,
                TransactionId = activeTransactionId,
                TransactionNumber = command.ActiveTransactioNumber + 1
            });

            return CommandHandlingResult.Ok();
        }
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Contract.Events;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.OperationExecutions;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands.OperationExecution;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.CommandHandlers.OperationExecution
{
    [UsedImplicitly]
    public class NotifyOperationExecutionCompletedCommandsHandler
    {
        [UsedImplicitly]
        public Task<CommandHandlingResult> Handle(NotifyOperationExecutionCompletedCommand command, IEventPublisher publisher)
        {
            switch (command.EndpointsConfiguration)
            {
                case OperationExecutionEndpointsConfiguration.OneToMany:
                    publisher.PublishEvent
                    (
                        new OneToManyOperationExecutionCompletedEvent
                        {
                            OperationId = command.OperationId,
                            TransactionId = command.TransactionId,
                            TransactionHash = command.TransactionHash,
                            TransactionOutputs = command.TransactionOutputs,
                            Fee = command.TransactionFee,
                            Block = command.TransactionBlock
                        }
                    );
                    break;

                case OperationExecutionEndpointsConfiguration.OneToOne:

                    publisher.PublishEvent
                    (
                        new OperationExecutionCompletedEvent
                        {
                            OperationId = command.OperationId,
                            TransactionId = command.TransactionId,
                            TransactionHash = command.TransactionHash,
                            TransactionAmount = command.TransactionOutputs.Single().Amount,
                            Fee = command.TransactionFee,
                            Block = command.TransactionBlock
                        }
                    );
                    break;

                default:
                    throw new InvalidOperationException("There should be at least one output");
            }

            return Task.FromResult(CommandHandlingResult.Ok());
        }
    }
}

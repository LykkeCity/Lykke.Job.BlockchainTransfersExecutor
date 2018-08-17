using System;
using System.Linq;
using Lykke.Job.BlockchainOperationsExecutor.Contract.Events;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.OperationExecutions;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.TransactionExecutions;
using Lykke.Job.BlockchainOperationsExecutor.StateMachine;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.OperationExecution;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.TransactionExecution;
using Xunit;

namespace Lykke.Job.BlockchainOperationsExecutor.Tests
{
    public class OperationExecutionStateMachineTests
    {
        [Fact]
        public void Test_Simple_Execution_To_Completion()
        {
            // Arrange

            var switcher = OperationExecutionStateSwitcherBuilder.Build();
            var aggregate = OperationExecutionAggregate.Start
            (
                Guid.NewGuid(),
                "",
                "",
                "",
                0,
                false,
                "",
                ""
            );

            // Act / Assert

            Assert.Equal(OperationExecutionState.Started, aggregate.State);
            
            Assert.True(switcher.Switch(aggregate, new ActiveTransactionIdGeneratedEvent()));
            Assert.Equal(OperationExecutionState.ActiveTransactionIdGenerated, aggregate.State);
            
            Assert.True(switcher.Switch(aggregate, new TransactionExecutionStartedEvent()));
            Assert.Equal(OperationExecutionState.TransactionExecutionInProgress, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new TransactionExecutionCompletedEvent()));
            Assert.Equal(OperationExecutionState.Completed, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new OperationExecutionCompletedEvent()));
            Assert.Equal(OperationExecutionState.NotifiedAboutEnding, aggregate.State);
        }

        [Fact]
        public void Test_Simple_Execution_To_Failure()
        {
            // Arrange

            var switcher = OperationExecutionStateSwitcherBuilder.Build();
            var aggregate = OperationExecutionAggregate.Start
            (
                Guid.NewGuid(),
                "",
                "",
                "",
                0,
                false,
                "",
                ""
            );

            // Act / Assert

            Assert.Equal(OperationExecutionState.Started, aggregate.State);
            
            Assert.True(switcher.Switch(aggregate, new ActiveTransactionIdGeneratedEvent()));
            Assert.Equal(OperationExecutionState.ActiveTransactionIdGenerated, aggregate.State);
            
            Assert.True(switcher.Switch(aggregate, new TransactionExecutionStartedEvent()));
            Assert.Equal(OperationExecutionState.TransactionExecutionInProgress, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new TransactionExecutionFailedEvent
            {
                ErrorCode = TransactionExecutionResult.UnknownError
            }));
            Assert.Equal(OperationExecutionState.Failed, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new OperationExecutionFailedEvent()));
            Assert.Equal(OperationExecutionState.NotifiedAboutEnding, aggregate.State);
        }

        [Fact]
        public void Test_Execution_With_Double_Transaction_Repeat_To_Completion()
        {
            // Arrange

            var switcher = OperationExecutionStateSwitcherBuilder.Build();
            var aggregate = OperationExecutionAggregate.Start
            (
                Guid.NewGuid(),
                "",
                "",
                "",
                0,
                false,
                "",
                ""
            );

            // Act / Assert

            Assert.Equal(OperationExecutionState.Started, aggregate.State);
            
            Assert.True(switcher.Switch(aggregate, new ActiveTransactionIdGeneratedEvent()));
            Assert.Equal(OperationExecutionState.ActiveTransactionIdGenerated, aggregate.State);
            
            Assert.True(switcher.Switch(aggregate, new TransactionExecutionStartedEvent()));
            Assert.Equal(OperationExecutionState.TransactionExecutionInProgress, aggregate.State);

            foreach (var _ in Enumerable.Range(0, 2))
            {
                Assert.True(switcher.Switch(aggregate, new TransactionExecutionRepeatRequestedEvent
                {
                    ErrorCode = TransactionExecutionResult.RebuildingIsRequired
                }));
                Assert.Equal(OperationExecutionState.TransactionExecutionRepeatRequested, aggregate.State);

                Assert.True(switcher.Switch(aggregate, new ActiveTransactionClearedEvent()));
                Assert.Equal(OperationExecutionState.ActiveTransactionCleared, aggregate.State);

                Assert.True(switcher.Switch(aggregate, new ActiveTransactionIdGeneratedEvent()));
                Assert.Equal(OperationExecutionState.ActiveTransactionIdGenerated, aggregate.State);

                Assert.True(switcher.Switch(aggregate, new TransactionExecutionStartedEvent()));
                Assert.Equal(OperationExecutionState.TransactionExecutionInProgress, aggregate.State);    
            }
            
            Assert.True(switcher.Switch(aggregate, new TransactionExecutionCompletedEvent()));
            Assert.Equal(OperationExecutionState.Completed, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new OperationExecutionCompletedEvent()));
            Assert.Equal(OperationExecutionState.NotifiedAboutEnding, aggregate.State);
        }

        [Fact]
        public void Test_Execution_With_Double_Transaction_Repeat_To_Failure()
        {
            // Arrange

            var switcher = OperationExecutionStateSwitcherBuilder.Build();
            var aggregate = OperationExecutionAggregate.Start
            (
                Guid.NewGuid(),
                "",
                "",
                "",
                0,
                false,
                "",
                ""
            );

            // Act / Assert

            Assert.Equal(OperationExecutionState.Started, aggregate.State);
            
            Assert.True(switcher.Switch(aggregate, new ActiveTransactionIdGeneratedEvent()));
            Assert.Equal(OperationExecutionState.ActiveTransactionIdGenerated, aggregate.State);
            
            Assert.True(switcher.Switch(aggregate, new TransactionExecutionStartedEvent()));
            Assert.Equal(OperationExecutionState.TransactionExecutionInProgress, aggregate.State);

            foreach (var _ in Enumerable.Range(0, 2))
            {
                Assert.True(switcher.Switch(aggregate, new TransactionExecutionRepeatRequestedEvent
                {
                    ErrorCode = TransactionExecutionResult.RebuildingIsRequired
                }));
                Assert.Equal(OperationExecutionState.TransactionExecutionRepeatRequested, aggregate.State);

                Assert.True(switcher.Switch(aggregate, new ActiveTransactionClearedEvent()));
                Assert.Equal(OperationExecutionState.ActiveTransactionCleared, aggregate.State);

                Assert.True(switcher.Switch(aggregate, new ActiveTransactionIdGeneratedEvent()));
                Assert.Equal(OperationExecutionState.ActiveTransactionIdGenerated, aggregate.State);

                Assert.True(switcher.Switch(aggregate, new TransactionExecutionStartedEvent()));
                Assert.Equal(OperationExecutionState.TransactionExecutionInProgress, aggregate.State);    
            }
            
            Assert.True(switcher.Switch(aggregate, new TransactionExecutionFailedEvent
            {
                ErrorCode = TransactionExecutionResult.UnknownError
            }));
            Assert.Equal(OperationExecutionState.Failed, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new OperationExecutionFailedEvent()));
            Assert.Equal(OperationExecutionState.NotifiedAboutEnding, aggregate.State);
        }
    }
}

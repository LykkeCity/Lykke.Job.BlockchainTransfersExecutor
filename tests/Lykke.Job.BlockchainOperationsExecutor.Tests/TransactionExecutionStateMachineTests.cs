using System;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.TransactionExecutions;
using Lykke.Job.BlockchainOperationsExecutor.StateMachine;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.TransactionExecution;
using Xunit;

namespace Lykke.Job.BlockchainOperationsExecutor.Tests
{
    public class TransactionExecutionStateMachineTests
    {
        [Fact]
        public void Test_Execution_To_Completion()
        {
            // Arrange

            var switcher = TransitionExecutionStateSwitcherBuilder.Build();
            var aggregate = TransactionExecutionAggregate.Start
            (
                Guid.NewGuid(),
                Guid.NewGuid(),
                "",
                "",
                "",
                "",
                "",
                0,
                false
            );

            // Act / Assert

            Assert.Equal(TransactionExecutionState.Started, aggregate.State);
            
            Assert.True(switcher.Switch(aggregate, new SourceAddressLockedEvent()));
            Assert.Equal(TransactionExecutionState.SourceAddressLocked, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new TransactionBuiltEvent()));
            Assert.Equal(TransactionExecutionState.Built, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new TransactionSignedEvent()));
            Assert.Equal(TransactionExecutionState.Signed, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new TransactionBroadcastedEvent()));
            Assert.Equal(TransactionExecutionState.Broadcasted, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new SourceAddressLockReleasedEvent()));
            Assert.Equal(TransactionExecutionState.WaitingForEnding, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new TransactionExecutionCompletedEvent()));
            Assert.Equal(TransactionExecutionState.Completed, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new BroadcastedTransactionClearedEvent()));
            Assert.Equal(TransactionExecutionState.Cleared, aggregate.State);
        }

        [Fact]
        public void Test_Execution_To_Failue_On_Waiting_For_Transaction_Ending()
        {
            // Arrange

            var switcher = TransitionExecutionStateSwitcherBuilder.Build();
            var aggregate = TransactionExecutionAggregate.Start
            (
                Guid.NewGuid(),
                Guid.NewGuid(),
                "",
                "",
                "",
                "",
                "",
                0,
                false
            );

            // Act / Assert

            Assert.Equal(TransactionExecutionState.Started, aggregate.State);
            
            Assert.True(switcher.Switch(aggregate, new SourceAddressLockedEvent()));
            Assert.Equal(TransactionExecutionState.SourceAddressLocked, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new TransactionBuiltEvent()));
            Assert.Equal(TransactionExecutionState.Built, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new TransactionSignedEvent()));
            Assert.Equal(TransactionExecutionState.Signed, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new TransactionBroadcastedEvent()));
            Assert.Equal(TransactionExecutionState.Broadcasted, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new SourceAddressLockReleasedEvent()));
            Assert.Equal(TransactionExecutionState.WaitingForEnding, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new TransactionExecutionFailedEvent
            {
                ErrorCode = TransactionExecutionResult.UnknownError
            }));
            Assert.Equal(TransactionExecutionState.WaitingForEndingFailed, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new BroadcastedTransactionClearedEvent()));
            Assert.Equal(TransactionExecutionState.Cleared, aggregate.State);
        }

        [Fact]
        public void Test_Execution_To_Repeat_On_Waiting_For_Transaction_Ending()
        {
            // Arrange

            var switcher = TransitionExecutionStateSwitcherBuilder.Build();
            var aggregate = TransactionExecutionAggregate.Start
            (
                Guid.NewGuid(),
                Guid.NewGuid(),
                "",
                "",
                "",
                "",
                "",
                0,
                false
            );

            // Act / Assert

            Assert.Equal(TransactionExecutionState.Started, aggregate.State);
            
            Assert.True(switcher.Switch(aggregate, new SourceAddressLockedEvent()));
            Assert.Equal(TransactionExecutionState.SourceAddressLocked, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new TransactionBuiltEvent()));
            Assert.Equal(TransactionExecutionState.Built, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new TransactionSignedEvent()));
            Assert.Equal(TransactionExecutionState.Signed, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new TransactionBroadcastedEvent()));
            Assert.Equal(TransactionExecutionState.Broadcasted, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new SourceAddressLockReleasedEvent()));
            Assert.Equal(TransactionExecutionState.WaitingForEnding, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new TransactionExecutionRepeatRequestedEvent
            {
                ErrorCode = TransactionExecutionResult.RebuildingIsRequired
            }));
            Assert.Equal(TransactionExecutionState.WaitingForEndingFailed, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new BroadcastedTransactionClearedEvent()));
            Assert.Equal(TransactionExecutionState.Cleared, aggregate.State);
        }

        [Fact]
        public void Test_Execution_To_Failue_On_Broadcasting()
        {
            // Arrange

            var switcher = TransitionExecutionStateSwitcherBuilder.Build();
            var aggregate = TransactionExecutionAggregate.Start
            (
                Guid.NewGuid(),
                Guid.NewGuid(),
                "",
                "",
                "",
                "",
                "",
                0,
                false
            );

            // Act / Assert

            Assert.Equal(TransactionExecutionState.Started, aggregate.State);
            
            Assert.True(switcher.Switch(aggregate, new SourceAddressLockedEvent()));
            Assert.Equal(TransactionExecutionState.SourceAddressLocked, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new TransactionBuiltEvent()));
            Assert.Equal(TransactionExecutionState.Built, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new TransactionSignedEvent()));
            Assert.Equal(TransactionExecutionState.Signed, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new TransactionExecutionFailedEvent
            {
                ErrorCode = TransactionExecutionResult.UnknownError
            }));
            Assert.Equal(TransactionExecutionState.BroadcastingFailed, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new SourceAddressLockReleasedEvent()));
            Assert.Equal(TransactionExecutionState.SourceAddressReleased, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new BroadcastedTransactionClearedEvent()));
            Assert.Equal(TransactionExecutionState.Cleared, aggregate.State);
        }

        [Fact]
        public void Test_Execution_To_Repeat_On_Broadcasting()
        {
            // Arrange

            var switcher = TransitionExecutionStateSwitcherBuilder.Build();
            var aggregate = TransactionExecutionAggregate.Start
            (
                Guid.NewGuid(),
                Guid.NewGuid(),
                "",
                "",
                "",
                "",
                "",
                0,
                false
            );

            // Act / Assert

            Assert.Equal(TransactionExecutionState.Started, aggregate.State);
            
            Assert.True(switcher.Switch(aggregate, new SourceAddressLockedEvent()));
            Assert.Equal(TransactionExecutionState.SourceAddressLocked, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new TransactionBuiltEvent()));
            Assert.Equal(TransactionExecutionState.Built, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new TransactionSignedEvent()));
            Assert.Equal(TransactionExecutionState.Signed, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new TransactionExecutionRepeatRequestedEvent
            {
                ErrorCode = TransactionExecutionResult.RebuildingIsRequired
            }));
            Assert.Equal(TransactionExecutionState.BroadcastingFailed, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new SourceAddressLockReleasedEvent()));
            Assert.Equal(TransactionExecutionState.SourceAddressReleased, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new BroadcastedTransactionClearedEvent()));
            Assert.Equal(TransactionExecutionState.Cleared, aggregate.State);
        }

        [Fact]
        public void Test_Execution_To_Failue_On_Building()
        {
            // Arrange

            var switcher = TransitionExecutionStateSwitcherBuilder.Build();
            var aggregate = TransactionExecutionAggregate.Start
            (
                Guid.NewGuid(),
                Guid.NewGuid(),
                "",
                "",
                "",
                "",
                "",
                0,
                false
            );

            // Act / Assert

            Assert.Equal(TransactionExecutionState.Started, aggregate.State);
            
            Assert.True(switcher.Switch(aggregate, new SourceAddressLockedEvent()));
            Assert.Equal(TransactionExecutionState.SourceAddressLocked, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new TransactionExecutionFailedEvent
            {
                ErrorCode = TransactionExecutionResult.UnknownError
            }));
            Assert.Equal(TransactionExecutionState.BuildingFailed, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new SourceAddressLockReleasedEvent()));
            Assert.Equal(TransactionExecutionState.SourceAddressReleased, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new BroadcastedTransactionClearedEvent()));
            Assert.Equal(TransactionExecutionState.Cleared, aggregate.State);
        }
    }
}

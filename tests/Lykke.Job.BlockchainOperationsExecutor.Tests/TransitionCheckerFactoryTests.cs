using Lykke.Job.BlockchainOperationsExecutor.Contract.Events;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.TransactionExecutions;
using Lykke.Job.BlockchainOperationsExecutor.StateMachine;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.TransactionExecution;
using Xunit;

namespace Lykke.Job.BlockchainOperationsExecutor.Tests
{
    public class TransitionCheckerFactoryTests
    {
        [Fact]
        public void Can_Handle_TransactionBuiltEvent_Evt()
        {
            var stateMachine = TransitionExecutionStateSwitcherBuilder.Build();

            var transitionResult = stateMachine.CheckTransition(TransactionExecutionState.Started, new TransactionBuiltEvent());

            Assert.True(transitionResult.IsValid);
            Assert.Equal(TransactionExecutionState.TransactionIsBuilt, transitionResult.NextState);
        }

        [Fact]
        public void Can_Handle_TransactionSignedEvent_Evt()
        {
            var stateMachine = TransitionExecutionStateSwitcherBuilder.Build();

            var transitionResult = stateMachine.CheckTransition(TransactionExecutionState.TransactionIsBuilt, new TransactionSignedEvent());

            Assert.True(transitionResult.IsValid);
            Assert.Equal(TransactionExecutionState.TransactionIsSigned, transitionResult.NextState);
        }

        [Fact]
        public void Can_Handle_TransactionBroadcastedEvent_Evt()
        {
            var stateMachine = TransitionExecutionStateSwitcherBuilder.Build();

            var transitionResult = stateMachine.CheckTransition(TransactionExecutionState.TransactionIsSigned, new TransactionBroadcastedEvent());

            Assert.True(transitionResult.IsValid);
            Assert.Equal(TransactionExecutionState.TransactionIsBroadcasted, transitionResult.NextState);
        }

        [Fact]
        public void Can_Handle_SourceAddressLockReleasedEvent_Evt()
        {
            var stateMachine = TransitionExecutionStateSwitcherBuilder.Build();

            var transitionResult = stateMachine.CheckTransition(TransactionExecutionState.TransactionIsBroadcasted, new SourceAddressLockReleasedEvent());

            Assert.True(transitionResult.IsValid);
            Assert.Equal(TransactionExecutionState.IsSourceAddressReleased, transitionResult.NextState);
        }

        [Fact]
        public void Can_Handle_OperationExecutionCompletedEvent_Evt()
        {
            var stateMachine = TransitionExecutionStateSwitcherBuilder.Build();

            var transitionResult = stateMachine.CheckTransition(TransactionExecutionState.IsSourceAddressReleased, new OperationExecutionCompletedEvent());

            Assert.True(transitionResult.IsValid);
            Assert.Equal(TransactionExecutionState.TransactionIsFinished, transitionResult.NextState);
        }

        [Fact]
        public void Can_Handle_OperationExecutionFailedEvent_Evt()
        {
            var stateMachine = TransitionExecutionStateSwitcherBuilder.Build();

            var transitionResult = stateMachine.CheckTransition(TransactionExecutionState.IsSourceAddressReleased, new OperationExecutionFailedEvent());

            Assert.True(transitionResult.IsValid);
            Assert.Equal(TransactionExecutionState.TransactionIsFinished, transitionResult.NextState);
        }

        [Fact]
        public void Can_Handle_BroadcastedTransactionForgottenEvent_Evt()
        {
            var stateMachine = TransitionExecutionStateSwitcherBuilder.Build();

            var transitionResult = stateMachine.CheckTransition(TransactionExecutionState.TransactionIsFinished, new BroadcastedTransactionClearedEvent());

            Assert.True(transitionResult.IsValid);
            Assert.Equal(TransactionExecutionState.BroadcastedTransactionIsForgotten, transitionResult.NextState);
        }

        [Fact]
        public void Can_Handle_Multiple_Evt()
        {
            var stateMachine = TransitionExecutionStateSwitcherBuilder.Build();

            var transitionResult1 = stateMachine.CheckTransition(TransactionExecutionState.Started, new TransactionBuiltEvent());

            Assert.True(transitionResult1.IsValid);
            Assert.Equal(TransactionExecutionState.TransactionIsBuilt, transitionResult1.NextState);

            var transitionResult2 = stateMachine.CheckTransition(TransactionExecutionState.TransactionIsBuilt, new TransactionSignedEvent());

            Assert.True(transitionResult2.IsValid);
            Assert.Equal(TransactionExecutionState.TransactionIsSigned, transitionResult2.NextState);
        }

        [Fact]
        public void Can_Ignore_Previous_Event()
        {
            var stateMachine = TransitionExecutionStateSwitcherBuilder.Build();

            var transitionResult1 = stateMachine.CheckTransition(TransactionExecutionState.BroadcastedTransactionIsForgotten, new TransactionBuiltEvent());

            Assert.False(transitionResult1.IsValid);
            Assert.Equal(TransactionExecutionState.BroadcastedTransactionIsForgotten, transitionResult1.NextState);
        }

        [Fact]
        public void Can_Handle_Transaction_Building_Fail()
        {
            var stateMachine = TransitionExecutionStateSwitcherBuilder.Build();

            var checkTransition1 = stateMachine.CheckTransition(TransactionExecutionState.Started, new TransactionBuildingFailedEvent());

            Assert.True(checkTransition1.IsValid);
            Assert.Equal(TransactionExecutionState.TransactionBuildingFailed, checkTransition1.NextState);
        }

        [Fact]
        public void Can_Handle_Transaction_Broadcasting_Fail()
        {
            var stateMachine = TransitionExecutionStateSwitcherBuilder.Build();

            var checkTransition1 = stateMachine.CheckTransition(TransactionExecutionState.TransactionIsSigned, new TransactionBroadcastingFailedEvent());

            Assert.True(checkTransition1.IsValid);
            Assert.Equal(TransactionExecutionState.TransactionBroadcastingFailed, checkTransition1.NextState);
        }

        [Fact]
        public void Can_Release_Source_Address_On_Building_Fail()
        {
            var stateMachine = TransitionExecutionStateSwitcherBuilder.Build();

            var transitionResult1 = stateMachine.CheckTransition(TransactionExecutionState.TransactionBuildingFailed, new SourceAddressLockReleasedEvent());

            Assert.True(transitionResult1.IsValid);
            Assert.Equal(TransactionExecutionState.IsSourceAddressReleased, transitionResult1.NextState);
        }

        [Fact]
        public void Can_Release_Source_Address_On_Broadcasting_Fail()
        {
            var stateMachine = TransitionExecutionStateSwitcherBuilder.Build();

            var transitionResult1 = stateMachine.CheckTransition(TransactionExecutionState.TransactionBroadcastingFailed, new SourceAddressLockReleasedEvent());

            Assert.True(transitionResult1.IsValid);
            Assert.Equal(TransactionExecutionState.IsSourceAddressReleased, transitionResult1.NextState);
        }


        [Fact]
        public void Can_Restart_Transaction_Building()
        {
            var stateMachine = TransitionExecutionStateSwitcherBuilder.Build();

            var transitionResult1 = stateMachine.CheckTransition(TransactionExecutionState.Started, new TransactionBuiltEvent());

            Assert.True(transitionResult1.IsValid);
            Assert.Equal(TransactionExecutionState.TransactionIsBuilt, transitionResult1.NextState);
        }

        [Fact]
        public void Can_Release_Source_Address_Lock_On_Rebuild_Request_On_Broadcast()
        {
            var stateMachine = TransitionExecutionStateSwitcherBuilder.Build();

            var transitionResult1 = stateMachine.CheckTransition(TransactionExecutionState.TransactionIsSigned, new TransactionReBuildingIsRequestedOnBroadcastingEvent());

            Assert.True(transitionResult1.IsValid);
            Assert.Equal(TransactionExecutionState.BuildingRepeatIsRequestedOnBroadcasting, transitionResult1.NextState);
        }

        [Fact]
        public void Can_Restat_Rebuild_On_Broadcast_Request_Rebuilding()
        {
            var stateMachine = TransitionExecutionStateSwitcherBuilder.Build();

            var transitionResult1 = stateMachine.CheckTransition(TransactionExecutionState.BuildingRepeatIsRequestedOnBroadcasting, new TransactionReBuildingIsRequestedEvent());

            Assert.True(transitionResult1.IsValid);
            Assert.Equal(TransactionExecutionState.Started, transitionResult1.NextState);
        }



        [Fact]
        public void Can_Restart_Transaction_Building_After_Success_Broadcast()
        {
            var stateMachine = TransitionExecutionStateSwitcherBuilder.Build();

            var transitionResult1 = stateMachine.CheckTransition(TransactionExecutionState.IsSourceAddressReleased, new TransactionReBuildingIsRequestedEvent());

            Assert.True(transitionResult1.IsValid);
            Assert.Equal(TransactionExecutionState.Started, transitionResult1.NextState);
        }

        [Fact]
        public void Can_Release_Address_Lock_After_Build_Conflict()
        {
            var stateMachine = TransitionExecutionStateSwitcherBuilder.Build();

            var transitionResult1 = stateMachine.CheckTransition(TransactionExecutionState.Started, new SourceAddressLockReleasedEvent());

            Assert.True(transitionResult1.IsValid);
            Assert.Equal(TransactionExecutionState.IsSourceAddressReleased, transitionResult1.NextState);
        }
    }
}

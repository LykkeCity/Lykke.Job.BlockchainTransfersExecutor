using Lykke.Job.BlockchainOperationsExecutor.Contract.Events;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain;
using Lykke.Job.BlockchainOperationsExecutor.Services.Transitions;
using Xunit;

namespace Lykke.Job.BlockchainOperationsExecutor.Tests
{
    public class TransitionCheckerFactoryTests
    {
        [Fact]
        public void Can_Handle_TransactionBuiltEvent_Evt()
        {
            var stateMachine = TransitionCheckerFactory.BuildTransitionsForService();

            var transitionResult = stateMachine.CheckTransition(OperationExecutionState.Started, new TransactionBuiltEvent());

            Assert.True(transitionResult.IsValid);
            Assert.Equal(OperationExecutionState.TransactionIsBuilt, transitionResult.NextState);
        }

        [Fact]
        public void Can_Handle_TransactionSignedEvent_Evt()
        {
            var stateMachine = TransitionCheckerFactory.BuildTransitionsForService();

            var transitionResult = stateMachine.CheckTransition(OperationExecutionState.TransactionIsBuilt, new TransactionSignedEvent());

            Assert.True(transitionResult.IsValid);
            Assert.Equal(OperationExecutionState.TransactionIsSigned, transitionResult.NextState);
        }

        [Fact]
        public void Can_Handle_TransactionBroadcastedEvent_Evt()
        {
            var stateMachine = TransitionCheckerFactory.BuildTransitionsForService();

            var transitionResult = stateMachine.CheckTransition(OperationExecutionState.TransactionIsSigned, new TransactionBroadcastedEvent());

            Assert.True(transitionResult.IsValid);
            Assert.Equal(OperationExecutionState.TransactionIsBroadcasted, transitionResult.NextState);
        }

        [Fact]
        public void Can_Handle_SourceAddressLockReleasedEvent_Evt()
        {
            var stateMachine = TransitionCheckerFactory.BuildTransitionsForService();

            var transitionResult = stateMachine.CheckTransition(OperationExecutionState.TransactionIsBroadcasted, new SourceAddressLockReleasedEvent());

            Assert.True(transitionResult.IsValid);
            Assert.Equal(OperationExecutionState.SourceAddresIsReleased, transitionResult.NextState);
        }

        [Fact]
        public void Can_Handle_OperationExecutionCompletedEvent_Evt()
        {
            var stateMachine = TransitionCheckerFactory.BuildTransitionsForService();

            var transitionResult = stateMachine.CheckTransition(OperationExecutionState.SourceAddresIsReleased, new OperationExecutionCompletedEvent());

            Assert.True(transitionResult.IsValid);
            Assert.Equal(OperationExecutionState.TransactionIsFinished, transitionResult.NextState);
        }

        [Fact]
        public void Can_Handle_OperationExecutionFailedEvent_Evt()
        {
            var stateMachine = TransitionCheckerFactory.BuildTransitionsForService();

            var transitionResult = stateMachine.CheckTransition(OperationExecutionState.SourceAddresIsReleased, new OperationExecutionFailedEvent());

            Assert.True(transitionResult.IsValid);
            Assert.Equal(OperationExecutionState.TransactionIsFinished, transitionResult.NextState);
        }

        [Fact]
        public void Can_Handle_BroadcastedTransactionForgottenEvent_Evt()
        {
            var stateMachine = TransitionCheckerFactory.BuildTransitionsForService();

            var transitionResult = stateMachine.CheckTransition(OperationExecutionState.TransactionIsFinished, new BroadcastedTransactionForgottenEvent());

            Assert.True(transitionResult.IsValid);
            Assert.Equal(OperationExecutionState.BroadcastedTransactionIsForgotten, transitionResult.NextState);
        }

        [Fact]
        public void Can_Handle_Multiple_Evt()
        {
            var stateMachine = TransitionCheckerFactory.BuildTransitionsForService();

            var transitionResult1 = stateMachine.CheckTransition(OperationExecutionState.Started, new TransactionBuiltEvent());

            Assert.True(transitionResult1.IsValid);
            Assert.Equal(OperationExecutionState.TransactionIsBuilt, transitionResult1.NextState);

            var transitionResult2 = stateMachine.CheckTransition(OperationExecutionState.TransactionIsBuilt, new TransactionSignedEvent());

            Assert.True(transitionResult2.IsValid);
            Assert.Equal(OperationExecutionState.TransactionIsSigned, transitionResult2.NextState);
        }

        [Fact]
        public void Can_Ignore_Previous_Event()
        {
            var stateMachine = TransitionCheckerFactory.BuildTransitionsForService();

            var transitionResult1 = stateMachine.CheckTransition(OperationExecutionState.BroadcastedTransactionIsForgotten, new TransactionBuiltEvent());

            Assert.False(transitionResult1.IsValid);
            Assert.Equal(OperationExecutionState.BroadcastedTransactionIsForgotten, transitionResult1.NextState);
        }

        [Fact]
        public void Can_Handle_Transaction_Building_Fail()
        {
            var stateMachine = TransitionCheckerFactory.BuildTransitionsForService();

            var checkTransition1 = stateMachine.CheckTransition(OperationExecutionState.Started, new TransactionBuildingFailedEvent());

            Assert.True(checkTransition1.IsValid);
            Assert.Equal(OperationExecutionState.TransactionBuildingFailed, checkTransition1.NextState);
        }

        [Fact]
        public void Can_Handle_Transaction_Broadcasting_Fail()
        {
            var stateMachine = TransitionCheckerFactory.BuildTransitionsForService();

            var checkTransition1 = stateMachine.CheckTransition(OperationExecutionState.TransactionIsSigned, new TransactionBroadcastingFailed());

            Assert.True(checkTransition1.IsValid);
            Assert.Equal(OperationExecutionState.TransactionBroadcastingFailed, checkTransition1.NextState);
        }

        [Fact]
        public void Can_Release_Source_Address_On_Building_Fail()
        {
            var stateMachine = TransitionCheckerFactory.BuildTransitionsForService();

            var transitionResult1 = stateMachine.CheckTransition(OperationExecutionState.TransactionBuildingFailed, new SourceAddressLockReleasedEvent());

            Assert.True(transitionResult1.IsValid);
            Assert.Equal(OperationExecutionState.SourceAddresIsReleased, transitionResult1.NextState);
        }

        [Fact]
        public void Can_Release_Source_Address_On_Broadcasting_Fail()
        {
            var stateMachine = TransitionCheckerFactory.BuildTransitionsForService();

            var transitionResult1 = stateMachine.CheckTransition(OperationExecutionState.TransactionBroadcastingFailed, new SourceAddressLockReleasedEvent());

            Assert.True(transitionResult1.IsValid);
            Assert.Equal(OperationExecutionState.SourceAddresIsReleased, transitionResult1.NextState);
        }


        [Fact]
        public void Can_Restart_Transaction_Building()
        {
            var stateMachine = TransitionCheckerFactory.BuildTransitionsForService();

            var transitionResult1 = stateMachine.CheckTransition(OperationExecutionState.Started, new TransactionBuiltEvent());

            Assert.True(transitionResult1.IsValid);
            Assert.Equal(OperationExecutionState.TransactionIsBuilt, transitionResult1.NextState);
        }

        [Fact]
        public void Can_Restart_Transaction_Building_On_Broadcast()
        {
            var stateMachine = TransitionCheckerFactory.BuildTransitionsForService();

            var transitionResult1 = stateMachine.CheckTransition(OperationExecutionState.TransactionIsSigned, new TransactionReBuildingIsRequested());

            Assert.True(transitionResult1.IsValid);
            Assert.Equal(OperationExecutionState.Started, transitionResult1.NextState);
        }

        [Fact]
        public void Can_Restart_Transaction_Building_After_Success_Broadcast()
        {
            var stateMachine = TransitionCheckerFactory.BuildTransitionsForService();

            var transitionResult1 = stateMachine.CheckTransition(OperationExecutionState.SourceAddresIsReleased, new TransactionReBuildingIsRequested());

            Assert.True(transitionResult1.IsValid);
            Assert.Equal(OperationExecutionState.Started, transitionResult1.NextState);
        }
    }
}

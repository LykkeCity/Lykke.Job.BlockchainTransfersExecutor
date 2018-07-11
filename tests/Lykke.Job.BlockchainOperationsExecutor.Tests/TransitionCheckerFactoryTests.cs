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
    }
}

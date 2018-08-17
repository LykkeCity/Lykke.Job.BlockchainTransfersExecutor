using System;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.TransactionExecutions;
using Lykke.Job.BlockchainOperationsExecutor.StateMachine.Building;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.TransactionExecution;
using Xunit;

namespace Lykke.Job.BlockchainOperationsExecutor.Tests
{
    public class TransitionsTests
    {
        [Fact]
        public void Can_Proceed_Valid_Transaction()
        {
            var register = TransitionRegisterFactory.StartRegistrationFor<TransactionExecutionState>();

            register
                .From(TransactionExecutionState.Started)
                .On<TransactionBuiltEvent>()
                .HandleTransition(TransactionExecutionState.TransactionIsBuilt)
                
                .From(TransactionExecutionState.IsSourceAddressReleased)
                .On<SourceAddressLockReleasedEvent>()
                .HandleTransition(TransactionExecutionState.BroadcastedTransactionIsForgotten);

            register.In(TransactionExecutionState.Started)
                .Ignore<SourceAddressLockReleasedEvent>()
                .Ignore<BroadcastedTransactionClearedEvent>();

            var core = register.Build();

            var checkResult1 = core.Switch(TransactionExecutionState.Started, new TransactionBuiltEvent());

            Assert.True(checkResult1.IsValid);
            Assert.Equal(TransactionExecutionState.TransactionIsBuilt, checkResult1.HandleTransition);
        }

        [Fact]
        public void Can_Proceed_Valid_Transaction_Multiple_Register()
        {
            var register = TransitionRegisterFactory.StartRegistrationFor<TransactionExecutionState>();

            register.From(TransactionExecutionState.Started, outputs =>
            {
                outputs.On<TransactionBuiltEvent>()
                    .HandleTransition(TransactionExecutionState.TransactionIsBuilt);

                outputs.On<SourceAddressLockReleasedEvent>()
                    .HandleTransition(TransactionExecutionState.IsSourceAddressReleased);
            });

            var core = register.Build();

            var checkResult1 = core.Switch(TransactionExecutionState.Started, new TransactionBuiltEvent());

            Assert.True(checkResult1.IsValid);
            Assert.Equal(TransactionExecutionState.TransactionIsBuilt, checkResult1.HandleTransition);


            var checkResult2 = core.Switch(TransactionExecutionState.Started, new SourceAddressLockReleasedEvent());

            Assert.True(checkResult2.IsValid);
            Assert.Equal(TransactionExecutionState.IsSourceAddressReleased, checkResult2.HandleTransition);
        }

        [Fact]
        public void Throws_Exception_On_Unregistered_Event()
        {
            var register = TransitionRegisterFactory.StartRegistrationFor<TransactionExecutionState>();

            register
                .From(TransactionExecutionState.Started)
                .On<TransactionBuiltEvent>()
                .HandleTransition(TransactionExecutionState.TransactionIsBuilt)

                .From(TransactionExecutionState.IsSourceAddressReleased)
                .On<SourceAddressLockReleasedEvent>()
                .HandleTransition(TransactionExecutionState.IsSourceAddressReleased);

            register.In(TransactionExecutionState.Started)
                .Ignore<SourceAddressLockReleasedEvent>()
                .Ignore<BroadcastedTransactionClearedEvent>();

            var core = register.Build();


            Assert.Throws<ArgumentException>(() =>
            {
                core.Switch(TransactionExecutionState.IsSourceAddressReleased, new TransactionBuiltEvent());
            });
        }

        [Fact]
        public void Can_Ignore_Commands()
        {
            var register = TransitionRegisterFactory.StartRegistrationFor<TransactionExecutionState>();

            register
                .From(TransactionExecutionState.Started)
                .On<TransactionBuiltEvent>()
                .HandleTransition(TransactionExecutionState.TransactionIsBuilt)

                .From(TransactionExecutionState.IsSourceAddressReleased)
                .On<SourceAddressLockReleasedEvent>()
                .HandleTransition(TransactionExecutionState.IsSourceAddressReleased);


            register.In(TransactionExecutionState.Started)
                .Ignore<SourceAddressLockReleasedEvent>()
                .Ignore<BroadcastedTransactionClearedEvent>();


            var core = register.Build();

            var checkResult1 = core.Switch(TransactionExecutionState.Started, new SourceAddressLockReleasedEvent());

            Assert.False(checkResult1.IsValid);


            var checkResult2 = core.Switch(TransactionExecutionState.Started, new BroadcastedTransactionClearedEvent());
            
            Assert.False(checkResult2.IsValid);
        }

        [Fact]
        public void Thows_Exception_On_Transition_Duplication()
        {
            var register = TransitionRegisterFactory.StartRegistrationFor<TransactionExecutionState>();
            
            Assert.Throws<ArgumentException>(() =>
            {
                register
                    .From(TransactionExecutionState.Started)
                    .On<TransactionBuiltEvent>()
                    .HandleTransition(TransactionExecutionState.TransactionIsBuilt)

                    .From(TransactionExecutionState.Started)
                    .On<TransactionBuiltEvent>()
                    .HandleTransition(TransactionExecutionState.TransactionIsBuilt);
            });
        }

        [Fact]
        public void Thows_Exception_On_Transition_Configuration_Conflict()
        {
            var register = TransitionRegisterFactory.StartRegistrationFor<TransactionExecutionState>();

            register
                .From(TransactionExecutionState.Started)
                .On<TransactionBuiltEvent>()
                .HandleTransition(TransactionExecutionState.TransactionIsBuilt);

            Assert.Throws<ArgumentException>(() =>
            {
                register.In(TransactionExecutionState.Started)
                    .Ignore<TransactionBuiltEvent>();
            });
        }
    }
}

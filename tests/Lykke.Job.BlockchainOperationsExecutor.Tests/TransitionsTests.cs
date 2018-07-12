using Lykke.Job.BlockchainOperationsExecutor.Core.Domain;
using Lykke.Job.BlockchainOperationsExecutor.Services.Transitions;
using System;
using Lykke.Job.BlockchainOperationsExecutor.Contract.Events;
using Xunit;

namespace Lykke.Job.BlockchainOperationsExecutor.Tests
{
    public class TransitionsTests
    {
        [Fact]
        public void Can_Proceed_Valid_Transaction()
        {
            var register = TransitionRegisterFacade.StartRegistrationFor<OperationExecutionState>();

            register
                .From(OperationExecutionState.Started)
                .On<TransactionBuiltEvent>()
                .SwitchTo(OperationExecutionState.TransactionIsBuilt)
                
                .From(OperationExecutionState.SourceAddresIsReleased)
                .On<SourceAddressLockReleasedEvent>()
                .SwitchTo(OperationExecutionState.BroadcastedTransactionIsForgotten);

            register.In(OperationExecutionState.Started)
                .Ignore<SourceAddressLockReleasedEvent>()
                .Ignore<BroadcastedTransactionForgottenEvent>();

            var core = register.Build();

            var checkResult1 = core.CheckTransition(OperationExecutionState.Started, new TransactionBuiltEvent());

            Assert.True(checkResult1.IsValid);
            Assert.Equal(OperationExecutionState.TransactionIsBuilt, checkResult1.NextState);
        }

        [Fact]
        public void Can_Proceed_Valid_Transaction_Multiple_Register()
        {
            var register = TransitionRegisterFacade.StartRegistrationFor<OperationExecutionState>();

            register.From(OperationExecutionState.Started, outputs =>
            {
                outputs.On<TransactionBuiltEvent>()
                    .SwitchTo(OperationExecutionState.TransactionIsBuilt);

                outputs.On<SourceAddressLockReleasedEvent>()
                    .SwitchTo(OperationExecutionState.SourceAddresIsReleased);
            });

            var core = register.Build();

            var checkResult1 = core.CheckTransition(OperationExecutionState.Started, new TransactionBuiltEvent());

            Assert.True(checkResult1.IsValid);
            Assert.Equal(OperationExecutionState.TransactionIsBuilt, checkResult1.NextState);


            var checkResult2 = core.CheckTransition(OperationExecutionState.Started, new SourceAddressLockReleasedEvent());

            Assert.True(checkResult2.IsValid);
            Assert.Equal(OperationExecutionState.SourceAddresIsReleased, checkResult2.NextState);
        }

        [Fact]
        public void Throws_Exception_On_Unregistered_Event()
        {
            var register = TransitionRegisterFacade.StartRegistrationFor<OperationExecutionState>();

            register
                .From(OperationExecutionState.Started)
                .On<TransactionBuiltEvent>()
                .SwitchTo(OperationExecutionState.TransactionIsBuilt)

                .From(OperationExecutionState.SourceAddresIsReleased)
                .On<SourceAddressLockReleasedEvent>()
                .SwitchTo(OperationExecutionState.SourceAddresIsReleased);

            register.In(OperationExecutionState.Started)
                .Ignore<SourceAddressLockReleasedEvent>()
                .Ignore<BroadcastedTransactionForgottenEvent>();

            var core = register.Build();


            Assert.Throws<ArgumentException>(() =>
            {
                core.CheckTransition(OperationExecutionState.SourceAddresIsReleased, new TransactionBuiltEvent());
            });
        }

        [Fact]
        public void Can_Ignore_Commands()
        {
            var register = TransitionRegisterFacade.StartRegistrationFor<OperationExecutionState>();

            register
                .From(OperationExecutionState.Started)
                .On<TransactionBuiltEvent>()
                .SwitchTo(OperationExecutionState.TransactionIsBuilt)

                .From(OperationExecutionState.SourceAddresIsReleased)
                .On<SourceAddressLockReleasedEvent>()
                .SwitchTo(OperationExecutionState.SourceAddresIsReleased);


            register.In(OperationExecutionState.Started)
                .Ignore<SourceAddressLockReleasedEvent>()
                .Ignore<BroadcastedTransactionForgottenEvent>();


            var core = register.Build();

            var checkResult1 = core.CheckTransition(OperationExecutionState.Started, new SourceAddressLockReleasedEvent());

            Assert.False(checkResult1.IsValid);


            var checkResult2 = core.CheckTransition(OperationExecutionState.Started, new BroadcastedTransactionForgottenEvent());
            
            Assert.False(checkResult2.IsValid);
        }

        [Fact]
        public void Thows_Exception_On_Transition_Duplication()
        {
            var register = TransitionRegisterFacade.StartRegistrationFor<OperationExecutionState>();
            
            Assert.Throws<ArgumentException>(() =>
            {
                register
                    .From(OperationExecutionState.Started)
                    .On<TransactionBuiltEvent>()
                    .SwitchTo(OperationExecutionState.TransactionIsBuilt)

                    .From(OperationExecutionState.Started)
                    .On<TransactionBuiltEvent>()
                    .SwitchTo(OperationExecutionState.TransactionIsBuilt);
            });
        }

        [Fact]
        public void Thows_Exception_On_Transition_Configuration_Conflict()
        {
            var register = TransitionRegisterFacade.StartRegistrationFor<OperationExecutionState>();

            register
                .From(OperationExecutionState.Started)
                .On<TransactionBuiltEvent>()
                .SwitchTo(OperationExecutionState.TransactionIsBuilt);

            Assert.Throws<ArgumentException>(() =>
            {
                register.In(OperationExecutionState.Started)
                    .Ignore<TransactionBuiltEvent>();
            });
        }
    }
}

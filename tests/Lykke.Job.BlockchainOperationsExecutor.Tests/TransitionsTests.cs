using Lykke.Job.BlockchainOperationsExecutor.Core.Domain;
using Lykke.Job.BlockchainOperationsExecutor.Services.Transitions;
using System;
using Lykke.Job.BlockchainOperationsExecutor.Contract.Events;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Events;
using Xunit;

namespace Lykke.Job.BlockchainOperationsExecutor.Tests
{
    public class TransitionsTests
    {
        [Fact]
        public void Can_Proceed_Valid_Transaction()
        {
            var register = TransitionRegisterFacade.StartRegistrationFor<TransactionExecutionState>();

            register
                .From(TransactionExecutionState.Started)
                .On<TransactionBuiltEvent>()
                .SwitchTo(TransactionExecutionState.TransactionIsBuilt)
                
                .From(TransactionExecutionState.IsSourceAddressReleased)
                .On<SourceAddressLockReleasedEvent>()
                .SwitchTo(TransactionExecutionState.BroadcastedTransactionIsForgotten);

            register.In(TransactionExecutionState.Started)
                .Ignore<SourceAddressLockReleasedEvent>()
                .Ignore<TransactionClearedEvent>();

            var core = register.Build();

            var checkResult1 = core.CheckTransition(TransactionExecutionState.Started, new TransactionBuiltEvent());

            Assert.True(checkResult1.IsValid);
            Assert.Equal(TransactionExecutionState.TransactionIsBuilt, checkResult1.NextState);
        }

        [Fact]
        public void Can_Proceed_Valid_Transaction_Multiple_Register()
        {
            var register = TransitionRegisterFacade.StartRegistrationFor<TransactionExecutionState>();

            register.From(TransactionExecutionState.Started, outputs =>
            {
                outputs.On<TransactionBuiltEvent>()
                    .SwitchTo(TransactionExecutionState.TransactionIsBuilt);

                outputs.On<SourceAddressLockReleasedEvent>()
                    .SwitchTo(TransactionExecutionState.IsSourceAddressReleased);
            });

            var core = register.Build();

            var checkResult1 = core.CheckTransition(TransactionExecutionState.Started, new TransactionBuiltEvent());

            Assert.True(checkResult1.IsValid);
            Assert.Equal(TransactionExecutionState.TransactionIsBuilt, checkResult1.NextState);


            var checkResult2 = core.CheckTransition(TransactionExecutionState.Started, new SourceAddressLockReleasedEvent());

            Assert.True(checkResult2.IsValid);
            Assert.Equal(TransactionExecutionState.IsSourceAddressReleased, checkResult2.NextState);
        }

        [Fact]
        public void Throws_Exception_On_Unregistered_Event()
        {
            var register = TransitionRegisterFacade.StartRegistrationFor<TransactionExecutionState>();

            register
                .From(TransactionExecutionState.Started)
                .On<TransactionBuiltEvent>()
                .SwitchTo(TransactionExecutionState.TransactionIsBuilt)

                .From(TransactionExecutionState.IsSourceAddressReleased)
                .On<SourceAddressLockReleasedEvent>()
                .SwitchTo(TransactionExecutionState.IsSourceAddressReleased);

            register.In(TransactionExecutionState.Started)
                .Ignore<SourceAddressLockReleasedEvent>()
                .Ignore<TransactionClearedEvent>();

            var core = register.Build();


            Assert.Throws<ArgumentException>(() =>
            {
                core.CheckTransition(TransactionExecutionState.IsSourceAddressReleased, new TransactionBuiltEvent());
            });
        }

        [Fact]
        public void Can_Ignore_Commands()
        {
            var register = TransitionRegisterFacade.StartRegistrationFor<TransactionExecutionState>();

            register
                .From(TransactionExecutionState.Started)
                .On<TransactionBuiltEvent>()
                .SwitchTo(TransactionExecutionState.TransactionIsBuilt)

                .From(TransactionExecutionState.IsSourceAddressReleased)
                .On<SourceAddressLockReleasedEvent>()
                .SwitchTo(TransactionExecutionState.IsSourceAddressReleased);


            register.In(TransactionExecutionState.Started)
                .Ignore<SourceAddressLockReleasedEvent>()
                .Ignore<TransactionClearedEvent>();


            var core = register.Build();

            var checkResult1 = core.CheckTransition(TransactionExecutionState.Started, new SourceAddressLockReleasedEvent());

            Assert.False(checkResult1.IsValid);


            var checkResult2 = core.CheckTransition(TransactionExecutionState.Started, new TransactionClearedEvent());
            
            Assert.False(checkResult2.IsValid);
        }

        [Fact]
        public void Thows_Exception_On_Transition_Duplication()
        {
            var register = TransitionRegisterFacade.StartRegistrationFor<TransactionExecutionState>();
            
            Assert.Throws<ArgumentException>(() =>
            {
                register
                    .From(TransactionExecutionState.Started)
                    .On<TransactionBuiltEvent>()
                    .SwitchTo(TransactionExecutionState.TransactionIsBuilt)

                    .From(TransactionExecutionState.Started)
                    .On<TransactionBuiltEvent>()
                    .SwitchTo(TransactionExecutionState.TransactionIsBuilt);
            });
        }

        [Fact]
        public void Thows_Exception_On_Transition_Configuration_Conflict()
        {
            var register = TransitionRegisterFacade.StartRegistrationFor<TransactionExecutionState>();

            register
                .From(TransactionExecutionState.Started)
                .On<TransactionBuiltEvent>()
                .SwitchTo(TransactionExecutionState.TransactionIsBuilt);

            Assert.Throws<ArgumentException>(() =>
            {
                register.In(TransactionExecutionState.Started)
                    .Ignore<TransactionBuiltEvent>();
            });
        }
    }
}

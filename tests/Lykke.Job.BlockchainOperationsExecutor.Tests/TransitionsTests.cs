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
            // Arrange

            var register = TransitionRegisterFactory.StartRegistrationFor<TransactionExecutionAggregate, TransactionExecutionState>();

            register.GetCurrentStateWith(a => a.State);

            register.From(TransactionExecutionState.Started)
                .On<SourceAddressLockedEvent>()
                .HandleTransition((a, e) => a.OnSourceAddressLocked());

            var core = register.Build();

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

            // Act

            var result = core.Switch(aggregate, new SourceAddressLockedEvent());

            // Assert

            Assert.True(result);
            Assert.Equal(TransactionExecutionState.SourceAddressLocked, aggregate.State);
        }
        
        [Fact]
        public void Can_Proceed_Valid_Transaction_Multiple_Register()
        {
            // Arrange

            var register = TransitionRegisterFactory.StartRegistrationFor<TransactionExecutionAggregate, TransactionExecutionState>();

            register.GetCurrentStateWith(a => a.State);

            register.From(TransactionExecutionState.SourceAddressLocked, outputs =>
            {
                outputs.On<TransactionBuiltEvent>()
                    .HandleTransition((a, e) => a.OnBuilt(e.FromAddressContext, e.TransactionContext));

                outputs.On<TransactionExecutionFailedEvent>()
                    .HandleTransition((a, e) => a.OnBuildingFailed(e.ErrorCode, e.Error));
            });

            var core = register.Build();

            var aggregate1 = TransactionExecutionAggregate.Start
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

            aggregate1.OnSourceAddressLocked();

            var aggregate2 = TransactionExecutionAggregate.Start
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

            aggregate2.OnSourceAddressLocked();

            // Act

            var result1 = core.Switch(aggregate1, new TransactionBuiltEvent());
            var result2 = core.Switch(aggregate2, new TransactionExecutionFailedEvent
            {
                ErrorCode = TransactionExecutionResult.UnknownError
            });

            // Assert

            Assert.True(result1);
            Assert.Equal(TransactionExecutionState.Built, aggregate1.State);

            Assert.True(result2);
            Assert.Equal(TransactionExecutionState.BuildingFailed, aggregate2.State);
        }

        [Fact]
        public void Throws_Exception_On_Unregistered_Event()
        {
            // Arrange

            var register = TransitionRegisterFactory.StartRegistrationFor<TransactionExecutionAggregate, TransactionExecutionState>();

            register.GetCurrentStateWith(a => a.State);

            register.From(TransactionExecutionState.Started)
                .On<SourceAddressLockedEvent>()
                .HandleTransition((a, e) => a.OnSourceAddressLocked());

            var core = register.Build();

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

            Assert.Throws<InvalidOperationException>(() =>
            {
                core.Switch(aggregate, new TransactionBuiltEvent());
            });
        }

        [Fact]
        public void Can_Ignore_Events()
        {
            // Arrange

            var register = TransitionRegisterFactory.StartRegistrationFor<TransactionExecutionAggregate, TransactionExecutionState>();

            register.GetCurrentStateWith(a => a.State);

            register.From(TransactionExecutionState.Started)
                .On<SourceAddressLockedEvent>()
                .HandleTransition((a, e) => a.OnSourceAddressLocked());

            register.In(TransactionExecutionState.Started)
                .Ignore<TransactionExecutionStartedEvent>();

            register.In(TransactionExecutionState.SourceAddressLocked)
                .Ignore<TransactionExecutionStartedEvent>()
                .Ignore<SourceAddressLockedEvent>();

            var core = register.Build();

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

            // Act

            var result1 = core.Switch(aggregate, new TransactionExecutionStartedEvent());
            var result2 = core.Switch(aggregate, new SourceAddressLockedEvent());
            var result3 = core.Switch(aggregate, new TransactionExecutionStartedEvent());
            var result4 = core.Switch(aggregate, new SourceAddressLockedEvent());
            
            // Assert

            Assert.False(result1);
            Assert.True(result2);
            Assert.False(result3);
            Assert.False(result4);
        }

        [Fact]
        public void Thows_Exception_On_Transition_Duplication()
        {
            // Arrange

            var register = TransitionRegisterFactory.StartRegistrationFor<TransactionExecutionAggregate, TransactionExecutionState>();

            Assert.Throws<ArgumentException>(() =>
            {
                register.From(TransactionExecutionState.Started)
                    .On<SourceAddressLockedEvent>()
                    .HandleTransition((a, e) => a.OnSourceAddressLocked());

                register.From(TransactionExecutionState.Started)
                    .On<SourceAddressLockedEvent>()
                    .HandleTransition((a, e) => a.OnSourceAddressLocked());
            });
        }

        [Fact]
        public void Thows_Exception_On_Transition_Configuration_Conflict()
        {
            var register = TransitionRegisterFactory.StartRegistrationFor<TransactionExecutionAggregate, TransactionExecutionState>();

            register.From(TransactionExecutionState.Started)
                .On<SourceAddressLockedEvent>()
                .HandleTransition((a, e) => a.OnSourceAddressLocked());

            Assert.Throws<ArgumentException>(() =>
            {
                register.In(TransactionExecutionState.Started)
                    .Ignore<SourceAddressLockedEvent>();
            });
        }
    }
}

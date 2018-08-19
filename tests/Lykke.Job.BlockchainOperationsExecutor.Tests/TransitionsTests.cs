using System;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.TransactionExecutions;
using Lykke.Job.BlockchainOperationsExecutor.StateMachine.Building;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.TransactionExecution;
using Xunit;
using Xunit.Abstractions;

namespace Lykke.Job.BlockchainOperationsExecutor.Tests
{
    public class TransitionsTests
    {
        private readonly ITestOutputHelper _output;

        public TransitionsTests(ITestOutputHelper output)
        {
            _output = output;
        }

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
                0,
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
                0,
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
                0,
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
                0,
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
                0,
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
        public void Throws_Exception_On_Transition_Duplication()
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
        public void Throws_Exception_On_Transition_Configuration_Conflict()
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

        [Fact]
        public void Can_Ignore_Already_Registered_Transition_With_Additional_Conditions()
        {
            // Arrange

            var register = TransitionRegisterFactory.StartRegistrationFor<TransactionExecutionAggregate, TransactionExecutionState>();

            register.GetCurrentStateWith(a => a.State);

            register.From(TransactionExecutionState.Started)
                .On<SourceAddressLockedEvent>()
                .HandleTransition((a, e) => a.OnSourceAddressLocked());

            register.In(TransactionExecutionState.Started)
                .Ignore<SourceAddressLockedEvent>((a, e) => a.IncludeFee) 
                .Ignore<TransactionExecutionStartedEvent>();

            var core = register.Build();

            var aggregate1 = TransactionExecutionAggregate.Start
            (
                Guid.NewGuid(),
                Guid.NewGuid(),
                0,
                "",
                "",
                "",
                "",
                "",
                0,
                false
            );

            var aggregate2 = TransactionExecutionAggregate.Start
            (
                Guid.NewGuid(),
                Guid.NewGuid(),
                0,
                "",
                "",
                "",
                "",
                "",
                0,
                true
            );

            // Act

            var result11 = core.Switch(aggregate1, new TransactionExecutionStartedEvent());
            var result12 = core.Switch(aggregate1, new SourceAddressLockedEvent());

            var result21 = core.Switch(aggregate2, new TransactionExecutionStartedEvent());
            var result22 = core.Switch(aggregate2, new SourceAddressLockedEvent());
            
            // Assert

            Assert.False(result11);
            Assert.True(result12);
            Assert.Equal(TransactionExecutionState.SourceAddressLocked, aggregate1.State);

            Assert.False(result21);
            Assert.False(result22);
            Assert.Equal(TransactionExecutionState.Started, aggregate2.State);
        }

        [Fact]
        public void Check_Transition_Preconditions()
        {
            // Arrange

            var register = TransitionRegisterFactory.StartRegistrationFor<TransactionExecutionAggregate, TransactionExecutionState>();

            register.GetCurrentStateWith(a => a.State);

            register.From(TransactionExecutionState.Started)
                .On<SourceAddressLockedEvent>()
                .WithPrecondition((a, e) => a.IncludeFee, (a, e) => "Include fee should be enabled")
                .WithPrecondition((a, e) => a.Amount > 0, (a, e) => "Amount should be positive number")
                .HandleTransition((a, e) => a.OnSourceAddressLocked());

            var core = register.Build();

            var aggregate1 = TransactionExecutionAggregate.Start
            (
                Guid.NewGuid(),
                Guid.NewGuid(),
                0,
                "",
                "",
                "",
                "",
                "",
                0,
                false
            );

            var aggregate2 = TransactionExecutionAggregate.Start
            (
                Guid.NewGuid(),
                Guid.NewGuid(),
                0,
                "",
                "",
                "",
                "",
                "",
                1,
                true
            );

            // Act

            Assert.Throws<InvalidOperationException>(() =>
            {
                try
                {
                    core.Switch(aggregate1, new SourceAddressLockedEvent());
                }
                catch (Exception e)
                {
                    _output.WriteLine(e.ToString());
                    throw;
                }
            });

            var result21 = core.Switch(aggregate2, new SourceAddressLockedEvent());
            
            // Assert

            Assert.Equal(TransactionExecutionState.Started, aggregate1.State);

            Assert.True(result21);
            Assert.Equal(TransactionExecutionState.SourceAddressLocked, aggregate2.State);
        }
    }
}

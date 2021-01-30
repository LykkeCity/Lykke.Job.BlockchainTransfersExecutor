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
                new []{new TransactionOutputValueType("", 0)},
                "",
                false,
                "",
                "",
                OperationExecutionEndpointsConfiguration.OneToOne
            );

            // Act / Assert

            Assert.Equal(OperationExecutionState.Started, aggregate.State);
            
            Assert.True(switcher.Switch(aggregate, new ActiveTransactionIdGeneratedEvent
            {
                TransactionNumber = 1
            }));
            Assert.Equal(OperationExecutionState.ActiveTransactionIdGenerated, aggregate.State);
            
            Assert.True(switcher.Switch(aggregate, new TransactionExecutionStartedEvent
            {
                TransactionNumber = 1
            }));
            Assert.Equal(OperationExecutionState.TransactionExecutionInProgress, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new TransactionExecutionCompletedEvent
            {
                TransactionNumber = 1
            }));
            Assert.Equal(OperationExecutionState.Completed, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new OperationExecutionCompletedEvent()));
            Assert.Equal(OperationExecutionState.NotifiedAboutEnding, aggregate.State);
        }

        [Fact]
        public void Test_One_To_Many_Execution_To_Completion()
        {
            // Arrange

            var switcher = OperationExecutionStateSwitcherBuilder.Build();
            var aggregate = OperationExecutionAggregate.Start
            (
                Guid.NewGuid(),
                "",
                new []
                {
                    new TransactionOutputValueType("1", 1.0m),
                    new TransactionOutputValueType("2", 2.0m), 
                },
                "",
                false,
                "",
                "",
                OperationExecutionEndpointsConfiguration.OneToMany
            );

            // Act / Assert

            Assert.Equal(OperationExecutionState.Started, aggregate.State);
            
            Assert.True(switcher.Switch(aggregate, new ActiveTransactionIdGeneratedEvent
            {
                TransactionNumber = 1
            }));
            Assert.Equal(OperationExecutionState.ActiveTransactionIdGenerated, aggregate.State);
            
            Assert.True(switcher.Switch(aggregate, new TransactionExecutionStartedEvent
            {
                TransactionNumber = 1
            }));
            Assert.Equal(OperationExecutionState.TransactionExecutionInProgress, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new TransactionExecutionCompletedEvent
            {
                TransactionNumber = 1
            }));
            Assert.Equal(OperationExecutionState.Completed, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new OneToManyOperationExecutionCompletedEvent()));
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
                new []{new TransactionOutputValueType("", 0)},
                "",
                false,
                "",
                "",
                OperationExecutionEndpointsConfiguration.OneToOne
            );

            // Act / Assert

            Assert.Equal(OperationExecutionState.Started, aggregate.State);
            
            Assert.True(switcher.Switch(aggregate, new ActiveTransactionIdGeneratedEvent
            {
                TransactionNumber = 1
            }));
            Assert.Equal(OperationExecutionState.ActiveTransactionIdGenerated, aggregate.State);
            
            Assert.True(switcher.Switch(aggregate, new TransactionExecutionStartedEvent
            {
                TransactionNumber = 1
            }));
            Assert.Equal(OperationExecutionState.TransactionExecutionInProgress, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new TransactionExecutionFailedEvent
            {
                ErrorCode = TransactionExecutionResult.UnknownError,
                TransactionNumber = 1
            }));
            Assert.Equal(OperationExecutionState.Failed, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new OperationExecutionFailedEvent()));
            Assert.Equal(OperationExecutionState.NotifiedAboutEnding, aggregate.State);
        }

        [Fact]
        public void Test_One_To_Many_Execution_To_Failure()
        {
            // Arrange

            var switcher = OperationExecutionStateSwitcherBuilder.Build();
            var aggregate = OperationExecutionAggregate.Start
            (
                Guid.NewGuid(),
                "",
                new []
                {
                    new TransactionOutputValueType("1", 1.0m),
                    new TransactionOutputValueType("2", 2.0m), 
                },
                "",
                false,
                "",
                "",
                OperationExecutionEndpointsConfiguration.OneToOne
            );

            // Act / Assert

            Assert.Equal(OperationExecutionState.Started, aggregate.State);
            
            Assert.True(switcher.Switch(aggregate, new ActiveTransactionIdGeneratedEvent
            {
                TransactionNumber = 1
            }));
            Assert.Equal(OperationExecutionState.ActiveTransactionIdGenerated, aggregate.State);
            
            Assert.True(switcher.Switch(aggregate, new TransactionExecutionStartedEvent
            {
                TransactionNumber = 1
            }));
            Assert.Equal(OperationExecutionState.TransactionExecutionInProgress, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new TransactionExecutionFailedEvent
            {
                ErrorCode = TransactionExecutionResult.UnknownError,
                TransactionNumber = 1
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
                new []{new TransactionOutputValueType("", 0)},
                "",
                false,
                "",
                "",
                OperationExecutionEndpointsConfiguration.OneToOne
            );

            // Act / Assert

            Assert.Equal(OperationExecutionState.Started, aggregate.State);
            
            Assert.True(switcher.Switch(aggregate, new ActiveTransactionIdGeneratedEvent
            {
                TransactionNumber = 1
            }));
            Assert.Equal(OperationExecutionState.ActiveTransactionIdGenerated, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new TransactionExecutionStartedEvent
            {
                TransactionNumber = 1
            }));
            Assert.Equal(OperationExecutionState.TransactionExecutionInProgress, aggregate.State);

            foreach (var transactionNumber in Enumerable.Range(2, 2))
            {
                Assert.True(switcher.Switch(aggregate, new TransactionExecutionRepeatRequestedEvent
                {
                    ErrorCode = TransactionExecutionResult.RebuildingIsRequired,
                    TransactionNumber = transactionNumber - 1
                }));
                Assert.Equal(OperationExecutionState.TransactionExecutionRepeatRequested, aggregate.State);

                Assert.True(switcher.Switch(aggregate, new ActiveTransactionClearedEvent
                {
                    TransactionNumber = transactionNumber - 1
                }));
                Assert.Equal(OperationExecutionState.ActiveTransactionCleared, aggregate.State);

                Assert.True(switcher.Switch(aggregate, new ActiveTransactionIdGeneratedEvent
                {
                    TransactionNumber = transactionNumber
                }));
                Assert.Equal(OperationExecutionState.ActiveTransactionIdGenerated, aggregate.State);

                Assert.True(switcher.Switch(aggregate, new TransactionExecutionStartedEvent
                {
                    TransactionNumber = transactionNumber
                }));
                Assert.Equal(OperationExecutionState.TransactionExecutionInProgress, aggregate.State);    
            }
            
            Assert.True(switcher.Switch(aggregate, new TransactionExecutionCompletedEvent
            {
                TransactionNumber = 3
            }));
            Assert.Equal(OperationExecutionState.Completed, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new OperationExecutionCompletedEvent()));
            Assert.Equal(OperationExecutionState.NotifiedAboutEnding, aggregate.State);
        }

        [Fact]
        public void Test_Execution_With_Manual_Rejection()
        {
            // Arrange

            var switcher = OperationExecutionStateSwitcherBuilder.Build();
            var aggregate = OperationExecutionAggregate.Start
            (
                Guid.NewGuid(),
                "",
                new[] { new TransactionOutputValueType("", 0) },
                "",
                false,
                "",
                "",
                OperationExecutionEndpointsConfiguration.OneToOne
            );

            // Act / Assert

            Assert.Equal(OperationExecutionState.Started, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new ActiveTransactionIdGeneratedEvent
            {
                TransactionNumber = 1
            }));
            Assert.Equal(OperationExecutionState.ActiveTransactionIdGenerated, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new TransactionExecutionStartedEvent
            {
                TransactionNumber = 1
            }));
            Assert.Equal(OperationExecutionState.TransactionExecutionInProgress, aggregate.State);

            foreach (var transactionNumber in Enumerable.Range(2, 2))
            {
                Assert.True(switcher.Switch(aggregate, new TransactionExecutionRepeatRequestedEvent
                {
                    ErrorCode = TransactionExecutionResult.RebuildingIsRequired,
                    TransactionNumber = transactionNumber - 1
                }));
                Assert.Equal(OperationExecutionState.TransactionExecutionRepeatRequested, aggregate.State);

                Assert.True(switcher.Switch(aggregate, new ActiveTransactionClearedEvent
                {
                    TransactionNumber = transactionNumber - 1
                }));
                Assert.Equal(OperationExecutionState.ActiveTransactionCleared, aggregate.State);

                Assert.True(switcher.Switch(aggregate, new ActiveTransactionIdGeneratedEvent
                {
                    TransactionNumber = transactionNumber
                }));
                Assert.Equal(OperationExecutionState.ActiveTransactionIdGenerated, aggregate.State);

                Assert.True(switcher.Switch(aggregate, new TransactionExecutionStartedEvent
                {
                    TransactionNumber = transactionNumber
                }));
                Assert.Equal(OperationExecutionState.TransactionExecutionInProgress, aggregate.State);
            }

            Assert.True(switcher.Switch(aggregate, new TransactionExecutionRepeatRequestedEvent
            {
                ErrorCode = TransactionExecutionResult.RebuildingIsRequired,
                TransactionNumber = 3
            }));
            Assert.Equal(OperationExecutionState.TransactionExecutionRepeatRequested, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new ActiveTransactionClearedEvent
            {
                TransactionNumber = 3
            }));

            Assert.Equal(OperationExecutionState.ActiveTransactionCleared, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new TransactionReBuildingRejectedEvent()));

            Assert.Equal(OperationExecutionState.Failed, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new OperationExecutionFailedEvent()));

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
                new []{new TransactionOutputValueType("", 0)},
                "",
                false,
                "",
                "",
                OperationExecutionEndpointsConfiguration.OneToOne
            );

            // Act / Assert

            Assert.Equal(OperationExecutionState.Started, aggregate.State);
            
            Assert.True(switcher.Switch(aggregate, new ActiveTransactionIdGeneratedEvent
            {
                TransactionNumber = 1
            }));
            Assert.Equal(OperationExecutionState.ActiveTransactionIdGenerated, aggregate.State);
            
            Assert.True(switcher.Switch(aggregate, new TransactionExecutionStartedEvent
            {
                TransactionNumber = 1
            }));
            Assert.Equal(OperationExecutionState.TransactionExecutionInProgress, aggregate.State);

            foreach (var transactionNumber in Enumerable.Range(2, 2))
            {
                Assert.True(switcher.Switch(aggregate, new TransactionExecutionRepeatRequestedEvent
                {
                    ErrorCode = TransactionExecutionResult.RebuildingIsRequired,
                    TransactionNumber = transactionNumber - 1
                }));
                Assert.Equal(OperationExecutionState.TransactionExecutionRepeatRequested, aggregate.State);

                Assert.True(switcher.Switch(aggregate, new ActiveTransactionClearedEvent
                {
                    TransactionNumber = transactionNumber - 1
                }));
                Assert.Equal(OperationExecutionState.ActiveTransactionCleared, aggregate.State);

                Assert.True(switcher.Switch(aggregate, new ActiveTransactionIdGeneratedEvent
                {
                    TransactionNumber = transactionNumber
                }));
                Assert.Equal(OperationExecutionState.ActiveTransactionIdGenerated, aggregate.State);

                Assert.True(switcher.Switch(aggregate, new TransactionExecutionStartedEvent
                {
                    TransactionNumber = transactionNumber
                }));
                Assert.Equal(OperationExecutionState.TransactionExecutionInProgress, aggregate.State);    
            }
            
            Assert.True(switcher.Switch(aggregate, new TransactionExecutionFailedEvent
            {
                ErrorCode = TransactionExecutionResult.UnknownError,
                TransactionNumber = 3
            }));
            Assert.Equal(OperationExecutionState.Failed, aggregate.State);

            Assert.True(switcher.Switch(aggregate, new OperationExecutionFailedEvent()));
            Assert.Equal(OperationExecutionState.NotifiedAboutEnding, aggregate.State);
        }
    }
}

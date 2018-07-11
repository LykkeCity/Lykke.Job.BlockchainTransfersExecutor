using System;
using System.Collections.Generic;
using System.Text;
using Lykke.Job.BlockchainOperationsExecutor.Contract.Events;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain;
using Lykke.Job.BlockchainOperationsExecutor.Services.Transitions.Interfaces;

namespace Lykke.Job.BlockchainOperationsExecutor.Services.Transitions
{
    public static class TransitionCheckerFactory
    {
        public static ITransitionChecker<OperationExecutionState> BuildTransitionsForService()
        {
            var register = TransitionRegisterFacade.StartRegistrationFor<OperationExecutionState>();

            register.From(OperationExecutionState.Started)
                .On<TransactionBuiltEvent>()
                .SwitchTo(OperationExecutionState.TransactionIsBuilt)
                .In(OperationExecutionState.Started)
                .Ignore<TransactionSignedEvent>()
                .Ignore<TransactionBroadcastedEvent>()
                .Ignore<SourceAddressLockReleasedEvent>()
                .Ignore<OperationExecutionCompletedEvent>()
                .Ignore<BroadcastedTransactionForgottenEvent>();

            register.From(OperationExecutionState.TransactionIsBuilt)
                .On<TransactionSignedEvent>()
                .SwitchTo(OperationExecutionState.TransactionIsSigned)

                .In(OperationExecutionState.TransactionIsBuilt)
                .Ignore<TransactionBroadcastedEvent>()
                .Ignore<SourceAddressLockReleasedEvent>()
                .Ignore<OperationExecutionCompletedEvent>()
                .Ignore<BroadcastedTransactionForgottenEvent>();

            register.From(OperationExecutionState.TransactionIsSigned)
                .On<TransactionBroadcastedEvent>()
                .SwitchTo(OperationExecutionState.TransactionIsBroadcasted)

                .In(OperationExecutionState.TransactionIsSigned)
                .Ignore<SourceAddressLockReleasedEvent>()
                .Ignore<OperationExecutionCompletedEvent>()
                .Ignore<BroadcastedTransactionForgottenEvent>();

            register.From(OperationExecutionState.TransactionIsBroadcasted)
                .On<SourceAddressLockReleasedEvent>()
                .SwitchTo(OperationExecutionState.SourceAddresIsReleased)

                .In(OperationExecutionState.TransactionIsBroadcasted)
                .Ignore<OperationExecutionCompletedEvent>()
                .Ignore<BroadcastedTransactionForgottenEvent>();

            register.From(OperationExecutionState.SourceAddresIsReleased, outputs =>
                {
                    outputs.On<OperationExecutionCompletedEvent>()
                        .SwitchTo(OperationExecutionState.TransactionIsFinished);

                    outputs.On<OperationExecutionFailedEvent>()
                        .SwitchTo(OperationExecutionState.TransactionIsFinished);
                })

                .In(OperationExecutionState.SourceAddresIsReleased)
                .Ignore<BroadcastedTransactionForgottenEvent>();


            register.From(OperationExecutionState.TransactionIsFinished)
                .On<BroadcastedTransactionForgottenEvent>()
                .SwitchTo(OperationExecutionState.BroadcastedTransactionIsForgotten);

            return register.Build();
        }
    }
}

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

            register.From(OperationExecutionState.Started, outputs =>
                    {
                        outputs.On<TransactionBuiltEvent>()
                            .SwitchTo(OperationExecutionState.TransactionIsBuilt);
                        outputs.On<TransactionBuildingFailedEvent>()
                            .SwitchTo(OperationExecutionState.TransactionBuildingFailed);
                    });

            register.From(OperationExecutionState.TransactionBuildingFailed)
                .On<SourceAddressLockReleasedEvent>()
                .SwitchTo(OperationExecutionState.SourceAddresIsReleased)

                .In(OperationExecutionState.TransactionBuildingFailed)
                .Ignore<TransactionBuildingFailedEvent>();

            register.From(OperationExecutionState.TransactionIsBuilt)
                .On<TransactionSignedEvent>()
                .SwitchTo(OperationExecutionState.TransactionIsSigned)

                .In(OperationExecutionState.TransactionIsBuilt)
                .Ignore<TransactionBuiltEvent>();

            register.From(OperationExecutionState.TransactionIsSigned, outputs =>
                {
                    outputs.On<TransactionBroadcastedEvent>()
                        .SwitchTo(OperationExecutionState.TransactionIsBroadcasted);

                    outputs.On<TransactionBroadcastingFailedEvent>()
                        .SwitchTo(OperationExecutionState.TransactionBroadcastingFailed);

                    outputs.On<TransactionReBuildingIsRequestedOnBroadcastingEvent>()
                        .SwitchTo(OperationExecutionState.TransactionRebuildingRequestedOnBroadcasting);
                })
                .In(OperationExecutionState.TransactionIsSigned)
                .Ignore<TransactionBuiltEvent>()
                .Ignore<TransactionSignedEvent>();

            register.From(OperationExecutionState.TransactionRebuildingRequestedOnBroadcasting)
                .On<TransactionReBuildingIsRequestedEvent>()
                .SwitchTo(OperationExecutionState.Started)
                .In(OperationExecutionState.TransactionRebuildingRequestedOnBroadcasting)
                .Ignore<TransactionReBuildingIsRequestedOnBroadcastingEvent>()
                .Ignore<TransactionBuiltEvent>()
                .Ignore<TransactionSignedEvent>();

            register.From(OperationExecutionState.TransactionIsBroadcasted)
                .On<SourceAddressLockReleasedEvent>()
                .SwitchTo(OperationExecutionState.SourceAddresIsReleased)

                .In(OperationExecutionState.TransactionIsBroadcasted)
                .Ignore<TransactionBuiltEvent>()
                .Ignore<TransactionSignedEvent>()
                .Ignore<TransactionBroadcastedEvent>();

            register.From(OperationExecutionState.TransactionBroadcastingFailed)
                .On<SourceAddressLockReleasedEvent>()
                .SwitchTo(OperationExecutionState.SourceAddresIsReleased)

                .In(OperationExecutionState.TransactionBroadcastingFailed)
                .Ignore<TransactionBuiltEvent>()
                .Ignore<TransactionSignedEvent>()
                .Ignore<TransactionBroadcastingFailedEvent>();
            


            register.From(OperationExecutionState.SourceAddresIsReleased, outputs =>
                {
                    outputs.On<OperationExecutionCompletedEvent>()
                        .SwitchTo(OperationExecutionState.TransactionIsFinished);

                    outputs.On<OperationExecutionFailedEvent>()
                        .SwitchTo(OperationExecutionState.TransactionIsFinished);

                    outputs.On<TransactionReBuildingIsRequestedEvent>()
                        .SwitchTo(OperationExecutionState.Started);
                })

                .In(OperationExecutionState.SourceAddresIsReleased)
                .Ignore<TransactionBuiltEvent>()
                .Ignore<TransactionSignedEvent>()
                .Ignore<TransactionBroadcastedEvent>()
                .Ignore<SourceAddressLockReleasedEvent>();


            register.From(OperationExecutionState.TransactionIsFinished)
                .On<BroadcastedTransactionForgottenEvent>()
                .SwitchTo(OperationExecutionState.BroadcastedTransactionIsForgotten)

                .In(OperationExecutionState.TransactionIsFinished)
                .Ignore<TransactionBuiltEvent>()
                .Ignore<TransactionSignedEvent>()
                .Ignore<TransactionBroadcastedEvent>()
                .Ignore<SourceAddressLockReleasedEvent>()
                .Ignore<OperationExecutionCompletedEvent>()
                .Ignore<OperationExecutionFailedEvent>();

            register.In(OperationExecutionState.BroadcastedTransactionIsForgotten)
                .Ignore<TransactionBuiltEvent>()
                .Ignore<TransactionSignedEvent>()
                .Ignore<TransactionBroadcastedEvent>()
                .Ignore<SourceAddressLockReleasedEvent>()
                .Ignore<OperationExecutionCompletedEvent>()
                .Ignore<OperationExecutionFailedEvent>()
                .Ignore<BroadcastedTransactionForgottenEvent>();

            return register.Build();
        }
    }
}

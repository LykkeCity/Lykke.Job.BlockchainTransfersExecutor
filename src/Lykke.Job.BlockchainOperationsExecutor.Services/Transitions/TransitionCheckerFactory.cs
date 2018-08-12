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
        public static ITransitionChecker<TransactionExecutionState> BuildTransitionsForService()
        {
            var register = TransitionRegisterFacade.StartRegistrationFor<TransactionExecutionState>();

            register.From(TransactionExecutionState.Started, outputs =>
                    {
                        outputs.On<TransactionBuiltEvent>()
                            .SwitchTo(TransactionExecutionState.TransactionIsBuilt);
                        outputs.On<TransactionBuildingFailedEvent>()
                            .SwitchTo(TransactionExecutionState.TransactionBuildingFailed);

                        //release source address lock after build conflict
                        outputs.On<SourceAddressLockReleasedEvent>()
                            .SwitchTo(TransactionExecutionState.IsSourceAddressReleased);
                    });

            register.From(TransactionExecutionState.TransactionIsBuilt)
                .On<TransactionSignedEvent>()
                .SwitchTo(TransactionExecutionState.TransactionIsSigned)

                .In(TransactionExecutionState.TransactionIsBuilt)
                .Ignore<TransactionBuiltEvent>();

            register.From(TransactionExecutionState.TransactionBuildingFailed)
                .On<SourceAddressLockReleasedEvent>()
                .SwitchTo(TransactionExecutionState.IsSourceAddressReleased)

                .In(TransactionExecutionState.TransactionBuildingFailed)
                .Ignore<TransactionBuildingFailedEvent>();

            register.From(TransactionExecutionState.TransactionIsSigned, outputs =>
                {
                    outputs.On<TransactionBroadcastedEvent>()
                        .SwitchTo(TransactionExecutionState.TransactionIsBroadcasted);

                    outputs.On<TransactionBroadcastingFailedEvent>()
                        .SwitchTo(TransactionExecutionState.TransactionBroadcastingFailed);

                    outputs.On<TransactionReBuildingIsRequestedOnBroadcastingEvent>()
                        .SwitchTo(TransactionExecutionState.BuildingRepeatIsRequestedOnBroadcasting);
                })
                .In(TransactionExecutionState.TransactionIsSigned)
                .Ignore<TransactionBuiltEvent>()
                .Ignore<TransactionSignedEvent>();

            register.From(TransactionExecutionState.TransactionIsBroadcasted)
                .On<SourceAddressLockReleasedEvent>()
                .SwitchTo(TransactionExecutionState.IsSourceAddressReleased)

                .In(TransactionExecutionState.TransactionIsBroadcasted)
                .Ignore<TransactionBuiltEvent>()
                .Ignore<TransactionSignedEvent>()
                .Ignore<TransactionBroadcastedEvent>();

            register.From(TransactionExecutionState.TransactionBroadcastingFailed)
                .On<SourceAddressLockReleasedEvent>()
                .SwitchTo(TransactionExecutionState.IsSourceAddressReleased)

                .In(TransactionExecutionState.TransactionBroadcastingFailed)
                .Ignore<TransactionBuiltEvent>()
                .Ignore<TransactionSignedEvent>()
                .Ignore<TransactionBroadcastingFailedEvent>();

            register.From(TransactionExecutionState.BuildingRepeatIsRequestedOnBroadcasting)
                .On<TransactionReBuildingIsRequestedEvent>()
                .SwitchTo(TransactionExecutionState.Started)
                .In(TransactionExecutionState.BuildingRepeatIsRequestedOnBroadcasting)
                .Ignore<TransactionBuiltEvent>()
                .Ignore<TransactionSignedEvent>()
                .Ignore<TransactionReBuildingIsRequestedOnBroadcastingEvent>();

            register.From(TransactionExecutionState.IsSourceAddressReleased, outputs =>
                {
                    outputs.On<OperationExecutionCompletedEvent>()
                        .SwitchTo(TransactionExecutionState.TransactionIsFinished);

                    outputs.On<OperationExecutionFailedEvent>()
                        .SwitchTo(TransactionExecutionState.TransactionIsFinished);

                    outputs.On<TransactionReBuildingIsRequestedEvent>()
                        .SwitchTo(TransactionExecutionState.Started);
                })

                .In(TransactionExecutionState.IsSourceAddressReleased)
                .Ignore<TransactionBuiltEvent>()
                .Ignore<TransactionSignedEvent>()
                .Ignore<TransactionBroadcastedEvent>()
                .Ignore<SourceAddressLockReleasedEvent>();


            register.From(TransactionExecutionState.TransactionIsFinished)
                .On<BroadcastedTransactionForgottenEvent>()
                .SwitchTo(TransactionExecutionState.BroadcastedTransactionIsForgotten)

                .In(TransactionExecutionState.TransactionIsFinished)
                .Ignore<TransactionBuiltEvent>()
                .Ignore<TransactionSignedEvent>()
                .Ignore<TransactionBroadcastedEvent>()
                .Ignore<SourceAddressLockReleasedEvent>()
                .Ignore<OperationExecutionCompletedEvent>()
                .Ignore<OperationExecutionFailedEvent>();

            register.In(TransactionExecutionState.BroadcastedTransactionIsForgotten)
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

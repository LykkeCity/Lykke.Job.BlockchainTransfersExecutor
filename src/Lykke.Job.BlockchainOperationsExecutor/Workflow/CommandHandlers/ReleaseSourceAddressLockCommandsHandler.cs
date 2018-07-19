﻿using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Contract.Events;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.CommandHandlers
{
    [UsedImplicitly]
    public class ReleaseSourceAddressLockCommandsHandler
    {
        private readonly IChaosKitty _chaosKitty;
        private readonly ISourceAddresLocksRepoistory _locksRepoistory;

        public ReleaseSourceAddressLockCommandsHandler(
            IChaosKitty chaosKitty,
            ISourceAddresLocksRepoistory locksRepoistory)
        {
            _chaosKitty = chaosKitty;
            _locksRepoistory = locksRepoistory;
        }

        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(ReleaseSourceAddressLockCommand command, IEventPublisher publisher)
        {
            await _locksRepoistory.ReleaseLockAsync(command.BlockchainType, command.FromAddress, command.OperationId);

            _chaosKitty.Meow(command.OperationId);

            if (command.BuildingRepeatsIsRequested)
            {
                publisher.PublishEvent(new TransactionReBuildingIsRequestedEvent
                {
                    OperationId = command.OperationId
                });
            }
            else
            {
                publisher.PublishEvent(new SourceAddressLockReleasedEvent
                {
                    OperationId = command.OperationId,
                    OperationExecutionErrorCode = command.OperationExecutionErrorCode
                });
            }


            return CommandHandlingResult.Ok();
        }
    }
}

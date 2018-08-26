using System;

namespace Lykke.Job.BlockchainOperationsExecutor.StateMachine.Building
{
    public interface ITransitionEventRegister<TAggregate, TState>: ITransitionRegisterBase<TAggregate>
        where TState : struct, IConvertible
    {
        TransitonHandlingRegister<TAggregate, TState, TEvent> On<TEvent>();
    }
}

using System;

namespace Lykke.Job.BlockchainOperationsExecutor.Services.Transitions.Interfaces.TransitionRegister
{
    public interface ITransitionEventRegister<TState>: ITransitionRegisterBase<TState>
        where TState : struct, Enum
    {
        ITransitonSwitchStateRegister<TState> On<TCommand>();
    }
}

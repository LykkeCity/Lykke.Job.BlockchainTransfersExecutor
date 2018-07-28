using System;

namespace Lykke.Job.BlockchainOperationsExecutor.Services.Transitions.Interfaces.TransitionRegister
{
    public interface ITransitionEventRegister<TState>: ITransitionRegisterBase<TState>
        where TState : struct, IConvertible
    {
        ITransitonSwitchStateRegister<TState> On<TCommand>();
    }
}

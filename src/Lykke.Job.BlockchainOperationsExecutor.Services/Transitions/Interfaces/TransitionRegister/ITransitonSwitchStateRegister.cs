using System;

namespace Lykke.Job.BlockchainOperationsExecutor.Services.Transitions.Interfaces.TransitionRegister
{
    public interface ITransitonSwitchStateRegister<TState> : ITransitionRegisterBase<TState>
        where TState : struct, Enum
    {
        ITransitionInitialStateRegister<TState> SwitchTo(TState state);
    }
}

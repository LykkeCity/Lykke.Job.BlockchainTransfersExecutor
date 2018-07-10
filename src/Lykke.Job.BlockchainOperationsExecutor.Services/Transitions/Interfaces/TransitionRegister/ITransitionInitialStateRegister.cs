using System;

namespace Lykke.Job.BlockchainOperationsExecutor.Services.Transitions.Interfaces.TransitionRegister
{
    public interface ITransitionInitialStateRegister<TState> : ITransitionRegisterBase<TState>
        where TState : struct, Enum
    {
        ITransitionInitialStateRegister<TState> From(TState initialState, Action<ITransitionEventRegister<TState>> registerTransition);
        ITransitionEventRegister<TState> From(TState initialState);
        ITransitionIgnoreRegister<TState> In(TState initialState);
    }
}

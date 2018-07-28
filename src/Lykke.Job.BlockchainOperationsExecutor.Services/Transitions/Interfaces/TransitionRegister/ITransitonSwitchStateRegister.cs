using System;

namespace Lykke.Job.BlockchainOperationsExecutor.Services.Transitions.Interfaces.TransitionRegister
{
    public interface ITransitonSwitchStateRegister<TState> : ITransitionRegisterBase<TState>
        where TState : struct, IConvertible
    {
        ITransitionInitialStateRegister<TState> SwitchTo(TState state);
    }
}

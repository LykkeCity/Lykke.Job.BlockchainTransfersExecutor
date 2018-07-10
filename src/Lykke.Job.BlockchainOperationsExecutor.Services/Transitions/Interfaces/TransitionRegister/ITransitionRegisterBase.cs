using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Job.BlockchainOperationsExecutor.Services.Transitions.Interfaces.TransitionRegister
{
    public interface ITransitionRegisterBase<TState>
        where TState : struct, Enum
    {
        ITransitionChecker<TState> Build();
    }
}

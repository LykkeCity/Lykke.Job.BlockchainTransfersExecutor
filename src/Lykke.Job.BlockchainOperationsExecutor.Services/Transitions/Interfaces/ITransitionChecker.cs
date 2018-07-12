using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Job.BlockchainOperationsExecutor.Services.Transitions.Interfaces
{
    public interface ITransitionChecker<TState> where TState : struct, Enum
    {
        TransitionCheckResult<TState> CheckTransition(TState currentState, object @event);
    }
}

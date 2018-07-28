using System;
using System.Collections.Generic;
using System.Text;
using Lykke.Job.BlockchainOperationsExecutor.Services.Transitions.Interfaces.TransitionRegister;

namespace Lykke.Job.BlockchainOperationsExecutor.Services.Transitions
{
    public static class TransitionRegisterFacade
    {
        public static ITransitionInitialStateRegister<TState> StartRegistrationFor<TState>() where TState : struct, IConvertible
        {
            return new TransitionRegister<TState>();
        }
    }
}

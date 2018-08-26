using System;

namespace Lykke.Job.BlockchainOperationsExecutor.StateMachine.Building
{
    public static class TransitionRegisterFactory
    {
        public static ITransitionInitialStateRegister<TAggregate, TState> StartRegistrationFor<TAggregate, TState>() 
            where TState : struct, IConvertible
        {
            return new TransitionRegister<TAggregate, TState>();
        }
    }
}

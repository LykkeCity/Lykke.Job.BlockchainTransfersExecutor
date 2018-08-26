using System;

namespace Lykke.Job.BlockchainOperationsExecutor.StateMachine.Building
{
    internal class TransitionRegistrationChain<TState> where TState : struct, IConvertible
    {
        public TState? State { get; set; }
        public Type EventType { get; set; }
    }
}

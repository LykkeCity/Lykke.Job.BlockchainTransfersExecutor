using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Job.BlockchainOperationsExecutor.Services.Transitions
{
    internal class TransitionRegistrationChain<TState> where TState : struct, IConvertible
    {
        public TState? State { get; set; }
        public Type EventType { get; set; }
    }
}

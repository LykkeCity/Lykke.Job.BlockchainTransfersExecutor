using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Job.BlockchainOperationsExecutor.Services.Transitions
{
    public class TransitionCheckResult<T> where T: struct, Enum  
    {
        public bool IsValid { get; }

        public T NextState { get; }

        public TransitionCheckResult(bool isValid, T nextState)
        {
            IsValid = isValid;
            NextState = nextState;
        }
    }
}

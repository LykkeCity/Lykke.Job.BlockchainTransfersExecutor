using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Job.BlockchainOperationsExecutor.Services.Transitions
{
    public class TransitionCheckResult<TState > where TState : struct, IConvertible
    {
        public bool IsValid { get; }

        public TState  NextState { get; }

        public TransitionCheckResult(bool isValid, TState  nextState)
        {
            if (!typeof(TState).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            IsValid = isValid;
            NextState = nextState;
        }
    }
}

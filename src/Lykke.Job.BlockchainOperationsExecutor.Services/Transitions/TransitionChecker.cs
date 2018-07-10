using System;
using System.Collections.Generic;
using System.Linq;
using Lykke.Job.BlockchainOperationsExecutor.Services.Transitions.Interfaces;

namespace Lykke.Job.BlockchainOperationsExecutor.Services.Transitions
{
    internal class TransitionChecker<TState>: ITransitionChecker<TState> 
        where TState :struct ,Enum
    {
        private readonly IDictionary<TransitionAttempt<TState>, TState> _validStateTransitions;
        private readonly IEnumerable<TransitionAttempt<TState>> _ignoredAttempts;


        public TransitionChecker(IDictionary<TransitionAttempt<TState>, TState> validStateTransitions, 
            IEnumerable<TransitionAttempt<TState>> ignoredTransitions)
        {
            _validStateTransitions = validStateTransitions;
            _ignoredAttempts = ignoredTransitions;
        }

        public CheckTransitionResultDto<TState> CheckTransition(TState currentState, object @event)
        {
            if (@event == null)
            {
                throw new ArgumentNullException();
            }

            var transitionState = new TransitionAttempt<TState>(currentState, @event.GetType());
            if (_validStateTransitions.ContainsKey(transitionState))
            {
                return new CheckTransitionResultDto<TState>
                {
                    IsValid = true,
                    NextState = _validStateTransitions[transitionState]
                };
            }

            if (_ignoredAttempts.Any(p => Equals(p, transitionState)))
            {
                return new CheckTransitionResultDto<TState>
                {
                    IsValid = false
                };
            }

            throw new ArgumentException($"Unknown transition switch: {currentState} {@event.GetType().Name}");
        }
    }
}

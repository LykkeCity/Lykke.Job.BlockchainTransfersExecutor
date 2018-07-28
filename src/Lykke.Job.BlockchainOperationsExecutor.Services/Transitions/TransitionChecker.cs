using System;
using System.Collections.Generic;
using System.Linq;
using Lykke.Job.BlockchainOperationsExecutor.Services.Transitions.Interfaces;

namespace Lykke.Job.BlockchainOperationsExecutor.Services.Transitions
{
    internal class TransitionChecker<TState>: ITransitionChecker<TState> 
        where TState : struct, IConvertible
    {
        private readonly IReadOnlyDictionary<TransitionRegistration<TState>, TState> _validStateTransitions;
        private readonly ISet<TransitionRegistration<TState>> _ignoredTransitions;


        public TransitionChecker(IDictionary<TransitionRegistration<TState>, TState> validStateTransitions, 
            ISet<TransitionRegistration<TState>> ignoredTransitions)
        {
            _validStateTransitions = validStateTransitions.ToDictionary(kv => kv.Key, kv => kv.Value);
            _ignoredTransitions = ignoredTransitions;
        }

        public TransitionCheckResult<TState> CheckTransition(TState currentState, object @event)
        {
            if (@event == null)
            {
                throw new ArgumentNullException();
            }

            var transitionState = new TransitionRegistration<TState>(currentState, @event.GetType());
            if (_validStateTransitions.ContainsKey(transitionState))
            {
                return new TransitionCheckResult<TState>(isValid:true, nextState: _validStateTransitions[transitionState]);
            }
            if (_ignoredTransitions.Any(p => Equals(p, transitionState)))
            {
                return new TransitionCheckResult<TState> (isValid:false, nextState:currentState);
            }

            throw new ArgumentException($"Unknown transition switch: {currentState} {@event.GetType().Name}");
        }
    }
}

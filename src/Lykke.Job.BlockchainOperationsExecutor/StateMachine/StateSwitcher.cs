using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Lykke.Job.BlockchainOperationsExecutor.StateMachine.Building;

namespace Lykke.Job.BlockchainOperationsExecutor.StateMachine
{
    internal class StateSwitcher<TAggregate, TState>: IStateSwitcher<TAggregate> 
        where TState : struct, IConvertible
    {
        private readonly IReadOnlyDictionary<TransitionRegistration<TState>, Delegate> _validStateTransitions;
        private readonly ISet<TransitionRegistration<TState>> _ignoredTransitions;
        private readonly Func<TAggregate, TState> _currentStateGetter;

        public StateSwitcher(
            IDictionary<TransitionRegistration<TState>, Delegate> validStateTransitions, 
            ISet<TransitionRegistration<TState>> ignoredTransitions,
            Func<TAggregate, TState> currentStateGetter)
        {
            _validStateTransitions = new ReadOnlyDictionary<TransitionRegistration<TState>, Delegate>(validStateTransitions);
            _ignoredTransitions = ignoredTransitions;
            _currentStateGetter = currentStateGetter;
        }

        public bool Switch(TAggregate aggregate, object @event)
        {
            if (aggregate == null)
            {
                throw new ArgumentNullException(nameof(aggregate));
            }

            if (@event == null)
            {
                throw new ArgumentNullException(nameof(@event));
            }

            var currentState = _currentStateGetter.Invoke(aggregate);
            var transitionState = new TransitionRegistration<TState>(currentState, @event.GetType());

            if (_validStateTransitions.TryGetValue(transitionState, out var transitionHandler))
            {
                transitionHandler.DynamicInvoke(aggregate, @event);

                return true;
            }
            
            if (_ignoredTransitions.Any(p => Equals(p, transitionState)))
            {
                return false;
            }

            throw new InvalidOperationException($"Unexpected event {@event.GetType().Name} in state {currentState}");
        }
    }
}

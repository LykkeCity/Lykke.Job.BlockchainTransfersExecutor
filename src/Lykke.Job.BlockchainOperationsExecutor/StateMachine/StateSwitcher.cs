using System;
using System.Collections.Generic;
using System.Linq;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain;
using Lykke.Job.BlockchainOperationsExecutor.StateMachine.Building;

namespace Lykke.Job.BlockchainOperationsExecutor.StateMachine
{
    internal class StateSwitcher<TAggregate, TState>: IStateSwitcher<TAggregate>
        where TState : struct, IConvertible
    {
        private readonly IReadOnlyDictionary<StateTransition<TState>, TransitionRegistration> _transitions;
        private readonly IReadOnlyDictionary<StateTransition<TState>, TransitionIgnoringRegistration> _ignoredTransitions;
        private readonly Func<TAggregate, TState> _currentStateGetter;

        public StateSwitcher(
            IReadOnlyDictionary<StateTransition<TState>, TransitionRegistration> transitions,
            IReadOnlyDictionary<StateTransition<TState>, TransitionIgnoringRegistration> ignoredTransitions,
            Func<TAggregate, TState> currentStateGetter)
        {
            _transitions = transitions;
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
            var transitionToProcess = new StateTransition<TState>(currentState, @event.GetType());

            if(_ignoredTransitions.TryGetValue(transitionToProcess, out var ignoredTransition))
            {
                if (ignoredTransition.IsAdditionalConditionsSatisfied(aggregate, @event))
                {
                    return false;
                }
            }

            if (_transitions.TryGetValue(transitionToProcess, out var transition))
            {
                var preconditionErrors = transition.GetPreconditionErrors(aggregate, @event);

                if (preconditionErrors.Any())
                {
                    var errorMessage = string.Join("\r\n\t- ", preconditionErrors);

                    throw new UnexpectedEventException(
                        $"Can't process event {@event.GetType().Name} in state {currentState} due to next preconditions:\r\n\t- {errorMessage}");
                }

                transition.Switch(aggregate, @event);

                return true;
            }

            throw new UnexpectedEventException($"Unexpected event {@event.GetType().Name} in state {currentState}");
        }
    }
}

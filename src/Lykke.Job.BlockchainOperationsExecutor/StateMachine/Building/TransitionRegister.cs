using System;
using System.Collections.Generic;
using System.Linq;

namespace Lykke.Job.BlockchainOperationsExecutor.StateMachine.Building
{
    internal class TransitionRegister<TAggregate, TState>:
        ITransitionInitialStateRegister<TAggregate, TState>, 
        ITransitionEventRegister<TAggregate, TState>, 
        ITransitionIgnoreRegister<TAggregate, TState>

        where TState : struct, IConvertible
    {
        private readonly IDictionary<TransitionRegistration<TState>, Delegate> _stateTransitionStorage;
        private readonly ISet<TransitionRegistration<TState>> _ignoredTransitionsStorage;
        private readonly TransitionRegistrationChain<TState> _registrationChain;

        private Func<TAggregate, TState> _currentStateGetter;

        public TransitionRegister()
        {
            _ignoredTransitionsStorage = new HashSet<TransitionRegistration<TState>>();
            _stateTransitionStorage = new Dictionary<TransitionRegistration<TState>, Delegate>();
            _registrationChain = new TransitionRegistrationChain<TState>();
        }

        public ITransitionInitialStateRegister<TAggregate, TState> GetCurrentStateWith(Func<TAggregate, TState> currentStateGetter)
        {
            _currentStateGetter = currentStateGetter ?? throw new ArgumentNullException(nameof(currentStateGetter));

            return this;
        }

        public ITransitionInitialStateRegister<TAggregate, TState> From(
            TState state, 
            Action<ITransitionEventRegister<TAggregate, TState>> registerTransition)
        {
            registerTransition(From(state));

            return this;
        }

        public ITransitionEventRegister<TAggregate, TState> From(TState state)
        {
            _registrationChain.State = state;

            return this;
        }

        public ITransitionIgnoreRegister<TAggregate, TState> In(TState state)
        {
            _registrationChain.State = state;

            return this;
        }

        public TransitonHandlingRegister<TAggregate, TState, TEvent> On<TEvent>()
        {
            _registrationChain.EventType = typeof(TEvent);

            return new TransitonHandlingRegister<TAggregate, TState, TEvent>(this);
        }

        internal void HandleTransition<TEvent>(Action<TAggregate, TEvent> handleTransition)
        {
            if (_registrationChain.State == null)
            {
                throw new InvalidOperationException("Initial state not registered");
            }

            if (_registrationChain.EventType == null)
            {
                throw new InvalidOperationException("Initial state not registered");
            }

            if (handleTransition == null)
            {
                throw new ArgumentNullException(nameof(handleTransition));
            }

            AddTransition(_registrationChain.State.Value, _registrationChain.EventType, handleTransition);
        }

        public ITransitionIgnoreRegister<TAggregate, TState> Ignore<TCommand>()
        {
            if (_registrationChain.State == null)
            {
                throw new InvalidOperationException("Initial state not registered");
            }

            AddIgnoredTransition(_registrationChain.State.Value, typeof(TCommand));

            return this;
        }

        public IStateSwitcher<TAggregate> Build()
        {
            if (_currentStateGetter == null)
            {
                throw new InvalidOperationException($"Set the getter of the aggregate current state to proceed with building. Use {nameof(GetCurrentStateWith)}");
            }

            if (!typeof(TState).IsEnum)
            {
                throw new InvalidOperationException($"{nameof(TState)} must be an enumerated type");
            }

            return new StateSwitcher<TAggregate, TState>(_stateTransitionStorage, _ignoredTransitionsStorage, _currentStateGetter);
        }

        private void AddTransition<TEvent>(TState initialState, Type eventType, Action<TAggregate, TEvent> handleTransition)
        {
            var transitionToAdd = new TransitionRegistration<TState>(initialState, eventType);

            ValidateTransitionDuplication(transitionToAdd);

            _stateTransitionStorage.Add(transitionToAdd, handleTransition);
        }

        private void AddIgnoredTransition(TState initialState, Type eventType)
        {
            var transitionToAdd = new TransitionRegistration<TState>(initialState, eventType);

            ValidateTransitionDuplication(transitionToAdd);

            _ignoredTransitionsStorage.Add(transitionToAdd);
        }

        private void ValidateTransitionDuplication(TransitionRegistration<TState> transition)
        {
            if (_stateTransitionStorage.ContainsKey(transition) 
                || _ignoredTransitionsStorage.Any(p => Equals(p, transition)))
            {
                throw new ArgumentException($"Transition: {transition.SourceState} {transition.GetType().Name} already registered");
            }
        }
    }
}

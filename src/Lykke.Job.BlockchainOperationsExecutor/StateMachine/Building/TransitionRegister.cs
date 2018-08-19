using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lykke.Job.BlockchainOperationsExecutor.StateMachine.Building
{
    internal class TransitionRegister<TAggregate, TState>:
        ITransitionInitialStateRegister<TAggregate, TState>, 
        ITransitionEventRegister<TAggregate, TState>, 
        ITransitionIgnoringRegister<TAggregate, TState>

        where TState : struct, IConvertible
    {
        private readonly IDictionary<StateTransition<TState>, TransitionRegistration> _stateTransitions;
        private readonly IDictionary<StateTransition<TState>, TransitionIgnoringRegistration> _ignoredTransitions;
        private readonly TransitionRegistrationChain<TState> _registrationChain;

        private Func<TAggregate, TState> _currentStateGetter;

        public TransitionRegister()
        {
            _ignoredTransitions = new Dictionary<StateTransition<TState>, TransitionIgnoringRegistration>();
            _stateTransitions = new Dictionary<StateTransition<TState>, TransitionRegistration>();
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

        public ITransitionIgnoringRegister<TAggregate, TState> In(TState state)
        {
            _registrationChain.State = state;

            return this;
        }

        public TransitonHandlingRegister<TAggregate, TState, TEvent> On<TEvent>()
        {
            _registrationChain.EventType = typeof(TEvent);

            return new TransitonHandlingRegister<TAggregate, TState, TEvent>(this);
        }

        internal void HandleTransition(Delegate handleTransition, IReadOnlyCollection<(Delegate Precondition, Delegate FormatMessage)> preconditions)
        {
            if (_registrationChain.State == null)
            {
                throw new InvalidOperationException("Current state is not initialized");
            }

            if (_registrationChain.EventType == null)
            {
                throw new InvalidOperationException("Current state is not initialized");
            }

            if (handleTransition == null)
            {
                throw new ArgumentNullException(nameof(handleTransition));
            }

            AddTransition(_registrationChain.State.Value, _registrationChain.EventType, handleTransition, preconditions);
        }

        public ITransitionIgnoringRegister<TAggregate, TState> Ignore<TEvent>()
        {
            if (_registrationChain.State == null)
            {
                throw new InvalidOperationException("Current state is not initialized");
            }

            AddTransitionIgnoring(_registrationChain.State.Value, typeof(TEvent), additionalCondition: null);

            return this;
        }

        public ITransitionIgnoringRegister<TAggregate, TState> Ignore<TEvent>(Func<TAggregate, TEvent, bool> additionalCondition)
        {
            if (_registrationChain.State == null)
            {
                throw new InvalidOperationException("Current state is not initialized");
            }

            AddTransitionIgnoring(_registrationChain.State.Value, typeof(TEvent), additionalCondition);

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

            return new StateSwitcher<TAggregate, TState>(
                new ReadOnlyDictionary<StateTransition<TState>, TransitionRegistration>(_stateTransitions), 
                new ReadOnlyDictionary<StateTransition<TState>, TransitionIgnoringRegistration>(_ignoredTransitions),
                _currentStateGetter);
        }

        private void AddTransition(
            TState sourceState, 
            Type eventType, 
            Delegate handleTransition, 
            IReadOnlyCollection<(Delegate Precondition, Delegate FormatMessage)> preconditions)
        {
            var transition = new StateTransition<TState>(sourceState, eventType);
            var transitionRegistration = new TransitionRegistration(handleTransition, preconditions);

            ValidateTransitionDuplication(transition);

            _stateTransitions.Add(transition, transitionRegistration);
        }

        private void AddTransitionIgnoring(TState initialState, Type eventType, Delegate additionalCondition)
        {
            var transition = new StateTransition<TState>(initialState, eventType);
            var transitionRegistration = new TransitionIgnoringRegistration(additionalCondition);

            ValidateTransitionDuplication(transition, transitionRegistration);

            _ignoredTransitions.Add(transition, transitionRegistration);
        }

        private void ValidateTransitionDuplication(StateTransition<TState> stateTransition)
        {
            if (_stateTransitions.ContainsKey(stateTransition))
            {
                throw new ArgumentException($"Transition {stateTransition} is already registered");
            }

            if (!_ignoredTransitions.TryGetValue(stateTransition, out var ignoringRegistration))
            {
                return;
            }

            if (!ignoringRegistration.HasAdditionalCondition)
            {
                throw new ArgumentException($"Transition {stateTransition} ignoring without additional condition is already registered as transition");
            }
        }

        private void ValidateTransitionDuplication(StateTransition<TState> stateTransition, TransitionIgnoringRegistration registration)
        {
            if (!registration.HasAdditionalCondition)
            {
                if (_stateTransitions.ContainsKey(stateTransition))
                {
                    throw new ArgumentException($"Transition {stateTransition} is already registered. Can't register transition ignoring without additional condition");
                }
            }

            if (_ignoredTransitions.ContainsKey(stateTransition))
            {
                throw new ArgumentException($"Transition {stateTransition} is already registered as transition ignoring");
            }
        }
    }
}

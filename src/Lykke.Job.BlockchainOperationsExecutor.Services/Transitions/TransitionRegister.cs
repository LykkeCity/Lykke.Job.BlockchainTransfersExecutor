using System;
using System.Collections.Generic;
using System.Linq;
using Lykke.Job.BlockchainOperationsExecutor.Services.Transitions.Interfaces;
using Lykke.Job.BlockchainOperationsExecutor.Services.Transitions.Interfaces.TransitionRegister;

namespace Lykke.Job.BlockchainOperationsExecutor.Services.Transitions
{
    internal class TransitionRegister<TState>:
        ITransitionInitialStateRegister<TState>, 
        ITransitionEventRegister<TState>, 
        ITransitonSwitchStateRegister<TState>,
        ITransitionIgnoreRegister<TState>
        where TState : struct, Enum
    {
        private readonly IDictionary<TransitionAttempt<TState>, TState> _stateTransitionStorage;
        private readonly IList<TransitionAttempt<TState>> _ignoredTransitionsStorage;

        private TState? _currentInitialState;
        private Type _currentEventType;

        public TransitionRegister()
        {
            _ignoredTransitionsStorage = new List<TransitionAttempt<TState>>();
            _stateTransitionStorage = new Dictionary<TransitionAttempt<TState>, TState>();
        }

        public ITransitionInitialStateRegister<TState> From(TState initialState, Action<ITransitionEventRegister<TState>> registerTransition)
        {
            registerTransition(From(initialState));

            return this;
        }

        public ITransitionEventRegister<TState> From(TState initialState)
        {
            _currentInitialState = initialState;

            return this;
        }

        public ITransitionIgnoreRegister<TState> In(TState initialState)
        {
            _currentInitialState = initialState;

            return this;
        }

        public ITransitonSwitchStateRegister<TState> On<TEvent>()
        {
            _currentEventType = typeof(TEvent);

            return this;
        }

        public ITransitionInitialStateRegister<TState> SwitchTo(TState state)
        {
            if (_currentInitialState == null)
            {
                throw new InvalidOperationException("Initial state not registered");
            };

            if (_currentEventType == null)
            {
                throw new InvalidOperationException("Initial state not registered");
            };

            AddTransition(_currentInitialState.Value, _currentEventType, state);
            
            return this;
        }
        public ITransitionIgnoreRegister<TState> Ignore<TCommand>()
        {
            if (_currentInitialState == null)
            {
                throw new InvalidOperationException("Initial state not registered");
            };

            AddIgnoredTransition(_currentInitialState.Value, typeof(TCommand));

            return this;
        }

        public ITransitionChecker<TState> Build()
        {
            return new TransitionChecker<TState>(_stateTransitionStorage, _ignoredTransitionsStorage);
        }

        private void AddTransition(TState initialState, Type eventType, TState nextState)
        {
            var transitionToAdd = new TransitionAttempt<TState>(initialState, eventType);

            ValidateTransitionDuplication(transitionToAdd);

            if (_stateTransitionStorage.ContainsKey(transitionToAdd))
            {
                throw new ArgumentException($"Transition {initialState}, {eventType} already registered");
            }

            _stateTransitionStorage.Add(transitionToAdd, nextState);
        }

        private void AddIgnoredTransition(TState initialState, Type eventType)
        {
            var transitionToAdd = new TransitionAttempt<TState>(initialState, eventType);

            ValidateTransitionDuplication(transitionToAdd);

            _ignoredTransitionsStorage.Add(transitionToAdd);
        }

        private void ValidateTransitionDuplication(TransitionAttempt<TState> transition)
        {
            if (_stateTransitionStorage.ContainsKey(transition) 
                || _ignoredTransitionsStorage.Any(p => Equals(p, transition)))
            {
                throw new ArgumentException($"Transition: {transition.InitialState} {transition.GetType().Name} already registered");
            }
        }
    }
}

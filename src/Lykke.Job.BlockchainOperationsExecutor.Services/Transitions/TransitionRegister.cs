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
        where TState : struct, IConvertible
    {
        private readonly IDictionary<TransitionRegistration<TState>, TState> _stateTransitionStorage;
        private readonly ISet<TransitionRegistration<TState>> _ignoredTransitionsStorage;

        private readonly TransitionRegistrationChain<TState> _registrationChain;

        public TransitionRegister()
        {
            _ignoredTransitionsStorage = new HashSet<TransitionRegistration<TState>>();
            _stateTransitionStorage = new Dictionary<TransitionRegistration<TState>, TState>();
            _registrationChain = new TransitionRegistrationChain<TState>();
        }

        public ITransitionInitialStateRegister<TState> From(TState initialState, Action<ITransitionEventRegister<TState>> registerTransition)
        {
            registerTransition(From(initialState));

            return this;
        }

        public ITransitionEventRegister<TState> From(TState initialState)
        {
            _registrationChain.State = initialState;

            return this;
        }

        public ITransitionIgnoreRegister<TState> In(TState initialState)
        {
            _registrationChain.State = initialState;

            return this;
        }

        public ITransitonSwitchStateRegister<TState> On<TEvent>()
        {
            _registrationChain.EventType = typeof(TEvent);

            return this;
        }

        public ITransitionInitialStateRegister<TState> SwitchTo(TState state)
        {
            if (_registrationChain.State == null)
            {
                throw new InvalidOperationException("Initial state not registered");
            };

            if (_registrationChain.EventType == null)
            {
                throw new InvalidOperationException("Initial state not registered");
            };

            AddTransition(_registrationChain.State.Value, _registrationChain.EventType, state);
            
            return this;
        }
        public ITransitionIgnoreRegister<TState> Ignore<TCommand>()
        {
            if (_registrationChain.State == null)
            {
                throw new InvalidOperationException("Initial state not registered");
            };

            AddIgnoredTransition(_registrationChain.State.Value, typeof(TCommand));

            return this;
        }

        public ITransitionChecker<TState> Build()
        {
            if (!typeof(TState).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            return new TransitionChecker<TState>(_stateTransitionStorage, _ignoredTransitionsStorage);
        }

        private void AddTransition(TState initialState, Type eventType, TState nextState)
        {
            var transitionToAdd = new TransitionRegistration<TState>(initialState, eventType);

            ValidateTransitionDuplication(transitionToAdd);

            if (_stateTransitionStorage.ContainsKey(transitionToAdd))
            {
                throw new ArgumentException($"Transition {initialState}, {eventType} already registered");
            }

            _stateTransitionStorage.Add(transitionToAdd, nextState);
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
                throw new ArgumentException($"Transition: {transition.InitialState} {transition.GetType().Name} already registered");
            }
        }
    }
}

using System;
using System.Collections.Generic;

namespace Lykke.Job.BlockchainOperationsExecutor.StateMachine.Building
{
    public class TransitonHandlingRegister<TAggregate, TState, TEvent> : ITransitionRegisterBase<TAggregate>
        where TState : struct, IConvertible
    {
        private readonly TransitionRegister<TAggregate, TState> _rootRegister;
        private readonly List<(Delegate Precondition, Delegate FormatMessage)> _preconditions;

        internal TransitonHandlingRegister(TransitionRegister<TAggregate, TState> rootRegister)
        {
            _rootRegister = rootRegister;
            _preconditions = new List<(Delegate, Delegate)>();
        }

        public TransitonHandlingRegister<TAggregate, TState, TEvent> WithPrecondition(
            Func<TAggregate, TEvent, bool> precondition, 
            Func<TAggregate, TEvent, string> formatMessage)
        {
            _preconditions.Add((Precondition: precondition, FormatMessage: formatMessage));

            return this;
        }

        public ITransitionInitialStateRegister<TAggregate, TState> HandleTransition(Action<TAggregate, TEvent> handleTransition)
        {
            _rootRegister.HandleTransition(handleTransition, _preconditions);

            return _rootRegister;
        }

        public IStateSwitcher<TAggregate> Build()
        {
            return _rootRegister.Build();
        }
    }
}

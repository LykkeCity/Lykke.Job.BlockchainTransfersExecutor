using System;

namespace Lykke.Job.BlockchainOperationsExecutor.StateMachine.Building
{
    public class TransitonHandlingRegister<TAggregate, TState, TEvent> : ITransitionRegisterBase<TAggregate>
        where TState : struct, IConvertible
    {
        private readonly TransitionRegister<TAggregate, TState> _rootRegister;

        internal TransitonHandlingRegister(TransitionRegister<TAggregate, TState> rootRegister)
        {
            _rootRegister = rootRegister;
        }

        public ITransitionInitialStateRegister<TAggregate, TState> HandleTransition(Action<TAggregate, TEvent> handleTransition)
        {
            _rootRegister.HandleTransition(handleTransition);

            return _rootRegister;
        }

        public IStateSwitcher<TAggregate> Build()
        {
            return _rootRegister.Build();
        }
    }
}

using System;

namespace Lykke.Job.BlockchainOperationsExecutor.StateMachine.Building
{
    public interface ITransitionInitialStateRegister<TAggregate, TState> : ITransitionRegisterBase<TAggregate>
        where TState : struct, IConvertible
    {
        ITransitionInitialStateRegister<TAggregate, TState> GetCurrentStateWith(Func<TAggregate, TState> currentStateGetter);
        ITransitionInitialStateRegister<TAggregate, TState> From(TState state, Action<ITransitionEventRegister<TAggregate, TState>> registerTransition);
        ITransitionEventRegister<TAggregate, TState> From(TState state);
        ITransitionIgnoringRegister<TAggregate, TState> In(TState state);
    }
}

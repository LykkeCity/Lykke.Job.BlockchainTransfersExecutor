using System;

namespace Lykke.Job.BlockchainOperationsExecutor.StateMachine.Building
{
    public interface ITransitionIgnoringRegister<TAggregate, TState> : ITransitionRegisterBase<TAggregate>
        where TState : struct, IConvertible
    {
        ITransitionIgnoringRegister<TAggregate, TState> Ignore<TEvent>();
        ITransitionIgnoringRegister<TAggregate, TState> Ignore<TEvent>(Func<TAggregate, TEvent, bool> additionalCondition);
    }
}

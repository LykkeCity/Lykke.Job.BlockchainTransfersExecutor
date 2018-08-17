using System;

namespace Lykke.Job.BlockchainOperationsExecutor.StateMachine.Building
{
    public interface ITransitionIgnoreRegister<in TAggregate, TState> : ITransitionRegisterBase<TAggregate>
        where TState : struct, IConvertible
    {
        ITransitionIgnoreRegister<TAggregate, TState> Ignore<TEvent>();
    }
}

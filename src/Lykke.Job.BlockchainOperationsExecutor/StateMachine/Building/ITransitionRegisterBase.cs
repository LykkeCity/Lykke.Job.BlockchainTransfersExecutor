namespace Lykke.Job.BlockchainOperationsExecutor.StateMachine.Building
{
    public interface ITransitionRegisterBase<in TAggregate>
    {
        IStateSwitcher<TAggregate> Build();
    }
}

namespace Lykke.Job.BlockchainOperationsExecutor.StateMachine
{
    public interface IStateSwitcher<in TAggregate>
    {
        bool Switch(TAggregate aggregate, object @event);
    }
}

using System.Threading.Tasks;

namespace Lykke.Job.BlockchainOperationsExecutor.AppServices.Lifecycle
{
    public interface IShutdownManager
    {
        Task StopAsync();
    }
}

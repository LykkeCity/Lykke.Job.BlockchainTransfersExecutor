using System.Threading.Tasks;

namespace Lykke.Job.BlockchainOperationsExecutor.AppServices.Lifecycle
{
    public interface IStartupManager
    {
        Task StartAsync();
    }
}

using System.Threading.Tasks;

namespace Lykke.Job.BlockchainOperationsExecutor.Core.Services
{
    public interface IStartupManager
    {
        Task StartAsync();
    }
}
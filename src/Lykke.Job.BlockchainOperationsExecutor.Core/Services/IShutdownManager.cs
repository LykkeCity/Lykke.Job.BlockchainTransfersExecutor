using System.Threading.Tasks;

namespace Lykke.Job.BlockchainOperationsExecutor.Core.Services
{
    public interface IShutdownManager
    {
        Task StopAsync();
    }
}
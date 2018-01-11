using System.Threading.Tasks;

namespace Lykke.Job.BlockchainTransfersExecutor.Core.Services
{
    public interface IStartupManager
    {
        Task StartAsync();
    }
}
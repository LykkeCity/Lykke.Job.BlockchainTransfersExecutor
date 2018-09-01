using JetBrains.Annotations;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Contract
{
    [PublicAPI]
    [MessagePackObject(keyAsPropertyName: true)]
    public class OperationOutput
    {
        public string Address { get; set; }

        public decimal Amount { get; set; }
    }
}

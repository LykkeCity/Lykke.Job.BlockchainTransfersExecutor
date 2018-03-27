using JetBrains.Annotations;

namespace Lykke.Job.BlockchainOperationsExecutor.Settings.SignFacade
{
    [UsedImplicitly]
    public class BlockchainSignFacadeClientSettings
    {
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public string ServiceUrl { get; set; }
    }
}

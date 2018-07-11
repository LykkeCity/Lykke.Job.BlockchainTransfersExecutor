using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Job.BlockchainOperationsExecutor.Services.Transitions
{
    public class CheckTransitionResultDto<T> where T: struct, Enum  
    {
        public bool IsValid { get; set; }

        public T NextState { get; set; }
    }
}

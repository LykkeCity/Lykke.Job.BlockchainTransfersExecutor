using System;

namespace Lykke.Job.BlockchainOperationsExecutor.Core.Domain
{
    public class UnexpectedEventException : Exception
    {
        public UnexpectedEventException()
        {
        }

        public UnexpectedEventException(string message):base(message)
        {
        }
    }
}

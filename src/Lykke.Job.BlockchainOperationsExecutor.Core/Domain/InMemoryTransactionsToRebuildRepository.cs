using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Lykke.Job.BlockchainOperationsExecutor.Core.Domain
{
    public class InMemoryTransactionsToRebuildRepository : ITransactionsToRebuildRepository
    {
        private readonly ConcurrentDictionary<Guid, bool> _storage = new();

        public Task<IReadOnlyCollection<Guid>> GetAll()
        {
            return Task.FromResult<IReadOnlyCollection<Guid>>(new ReadOnlyCollection<Guid>(_storage.Keys.ToList()));
        }

        public Task AddOrReplace(Guid operationId)
        {
            _storage.TryAdd(operationId, false);

            return Task.CompletedTask;
        }

        public Task EnsureRemoved(Guid operationId)
        {
            _storage.TryRemove(operationId, out _);

            return Task.CompletedTask;
        }

        public Task<bool> Contains(Guid operationId)
        {
            return Task.FromResult(_storage.ContainsKey(operationId));
        }
    }
}

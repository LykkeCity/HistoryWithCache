using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Job.OperationsCache.Models;

namespace Lykke.Job.OperationsCache.Services.InMemoryCache
{
    public class InMemoryStorage : IStorage
    {
        private readonly ConcurrentDictionary<string, CacheModel> _storage;

        public InMemoryStorage()
        {
            _storage = new ConcurrentDictionary<string, CacheModel>();
        }

        public async Task<IEnumerable<HistoryEntry>> Get(string clientId)
        {
            if (_storage.TryGetValue(clientId, out CacheModel cachedValue))
            {
                return cachedValue.Records;
            }

            return null;            
        }

        public async Task Set(string clientId, CacheModel cacheModel)
        {
            _storage.AddOrUpdate(clientId, cacheModel, (key, oldValue) => cacheModel);
        }
    }
}

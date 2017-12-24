using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Job.OperationsCache.Models;
using MessagePack;
using Microsoft.Extensions.Caching.Distributed;

namespace Lykke.Job.OperationsCache.Services.InMemoryCache
{
    public class RedisStorage : IStorage
    {
        private readonly IDistributedCache _redisCache;

        public RedisStorage(IDistributedCache redisCache)
        {
            _redisCache = redisCache;
        }

        public async Task<IEnumerable<HistoryEntry>> Get(string clientId)
        {
            var value = await _redisCache.GetAsync(GetCacheKey(clientId));
            if (value == null)
                return null;

            var cacheModel = MessagePackSerializer.Deserialize<CacheModel>(value);
            return cacheModel.Records;
        }

        public async Task Set(string clientId, CacheModel cacheModel)
        {
            var value = MessagePackSerializer.Serialize(cacheModel);
            await _redisCache.SetAsync(GetCacheKey(clientId), value);
        }

        private string GetCacheKey(string clientId)
        {
            return $"client:{clientId}:history";
        }
    }
}

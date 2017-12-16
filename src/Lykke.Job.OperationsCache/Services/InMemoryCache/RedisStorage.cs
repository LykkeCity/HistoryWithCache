using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Lykke.Job.OperationsCache.Models;
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
            string value = await _redisCache.GetStringAsync(GetCacheKey(clientId));

            return value?.DeserializeJson<CacheModel>().Records.Values.Select(v => v);
        }

        public async Task Set(string clientId, CacheModel cacheModel)
        {
            await _redisCache.SetStringAsync(GetCacheKey(clientId), cacheModel.ToJson());
        }

        private string GetCacheKey(string clientId)
        {
            return $"client:{clientId}:history";
        }
    }
}

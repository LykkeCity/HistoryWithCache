using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Job.OperationsCache.Models;

namespace Lykke.Job.OperationsCache.Services.InMemoryCache
{
    public interface IStorage
    {
        Task<IEnumerable<HistoryEntry>> Get(string clientId);
        Task Set(string clientId, CacheModel cacheModel);
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Job.OperationsCache.Core.Domain;

namespace Lykke.Job.OperationsCache.Core.Services
{
    public interface IStorage
    {
        Task<IEnumerable<HistoryEntry>> Get(string clientId);
        Task Set(string clientId, CacheModel cacheModel);
    }
}

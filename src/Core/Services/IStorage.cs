using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Domain;

namespace Core.Services
{
    public interface IStorage
    {
        Task<IEnumerable<HistoryEntry>> Get(string clientId);
        Task Set(string clientId, CacheModel cacheModel);
    }
}

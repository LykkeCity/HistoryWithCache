using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Job.OperationsCache.Models;

namespace Lykke.Job.OperationsCache.Services
{
    public interface IHistoryCache
    {
        Task<IEnumerable<HistoryEntry>> GetAllPagedAsync(string clientId, int page);
        
        Task WarmUp(string clientId, bool force = false);
    }
}

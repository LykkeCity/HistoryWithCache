using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Job.OperationsCache.Models;

namespace Lykke.Job.OperationsCache.Services
{
    public interface IHistoryCache
    {
        Task<IEnumerable<HistoryEntry>> GetAllPagedAsync(string clientId, int page);
        Task<IEnumerable<HistoryEntry>> GetAllAsync(string clientId, int top, int skip);
        Task<IEnumerable<HistoryEntry>> GetAllPagedAsync(string clientId, string assetId, string operationType, int page);
        Task<IEnumerable<HistoryEntry>> GetAllAsync(string clientId, string assetId, string operationType, int top, int skip);
        Task<IEnumerable<HistoryEntry>> GetAllByOpTypePagedAsync(string clientId, string operationType, int page);
        Task<IEnumerable<HistoryEntry>> GetAllByOpTypeAsync(string clientId, string operationType, int top, int skip);
        Task<IEnumerable<HistoryEntry>> GetAllByAssetPagedAsync(string clientId, string assetId, int page);
        Task<IEnumerable<HistoryEntry>> GetAllByAssetAsync(string clientId, string assetId, int top, int skip);

        void AddOrUpdate(HistoryEntry item);
        Task WarmUp(string clientId);
    }
}

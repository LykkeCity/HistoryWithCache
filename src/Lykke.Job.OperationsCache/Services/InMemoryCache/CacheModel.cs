using System.Collections.Concurrent;
using Lykke.Job.OperationsCache.Models;

namespace Lykke.Service.OperationsHistory.Services.InMemoryCache
{
    public class CacheModel
    {
        public ConcurrentDictionary<string, HistoryEntry> Records;
    }
}

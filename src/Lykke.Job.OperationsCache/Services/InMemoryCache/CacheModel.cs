using System.Collections.Concurrent;
using Lykke.Job.OperationsCache.Models;

namespace Lykke.Job.OperationsCache.Services.InMemoryCache
{
    public class CacheModel
    {
        public ConcurrentDictionary<string, HistoryEntry> Records;
    }
}

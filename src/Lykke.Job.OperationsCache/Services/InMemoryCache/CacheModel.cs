using System.Collections.Concurrent;
using System.Collections.Generic;
using Lykke.Job.OperationsCache.Models;

namespace Lykke.Job.OperationsCache.Services.InMemoryCache
{
    public class CacheModel
    {
        public Dictionary<string, HistoryEntry> Records;
    }
}

using System.Collections.Generic;
using Lykke.Job.OperationsCache.Models;
using MessagePack;

namespace Lykke.Job.OperationsCache.Services.InMemoryCache
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class CacheModel
    {
        public Dictionary<string, HistoryEntry> Records;
    }
}

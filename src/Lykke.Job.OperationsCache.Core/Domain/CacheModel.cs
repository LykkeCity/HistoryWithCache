using System.Collections.Generic;
using MessagePack;

namespace Lykke.Job.OperationsCache.Core.Domain
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class CacheModel
    {
        public List<HistoryEntry> Records;
    }
}

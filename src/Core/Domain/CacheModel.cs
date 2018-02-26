using System.Collections.Generic;
using MessagePack;

namespace Core.Domain
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class CacheModel
    {
        public List<HistoryEntry> Records;
    }
}

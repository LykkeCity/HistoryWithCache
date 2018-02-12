using System;
using MessagePack;

namespace Lykke.Job.OperationsCache.Core.Domain
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class HistoryEntry
    {
        public string Id { get; set; }
        public DateTime? DateTime { get; set; }
        public double? Amount { get; set; }
        public string Currency { get; set; }
        public string ClientId { get; set; }
        public string CustomData { get; set; }
        public string OpType { get; set; }
        public double FeeSize { get; set; }
        public string FeeType { get; set; }

        public override string ToString() => $"FeeSize: {FeeSize}, FeeType: {FeeType}";
    }
}

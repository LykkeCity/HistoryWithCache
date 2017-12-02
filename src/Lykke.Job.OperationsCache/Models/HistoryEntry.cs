﻿using System;

namespace Lykke.Job.OperationsCache.Models
{
    public class HistoryEntry
    {
        public string Id { get; set; }
        public DateTime DateTime { get; set; }
        public double Amount { get; set; }
        public string Currency { get; set; }
        public string ClientId { get; set; }
        public string CustomData { get; set; }
        public string OpType { get; set; }
    }
}

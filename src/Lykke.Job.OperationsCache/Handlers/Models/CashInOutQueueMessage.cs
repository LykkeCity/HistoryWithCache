using System;
using Newtonsoft.Json;

namespace Lykke.Job.OperationsCache.Handlers.Models
{
    public class CashInOutQueueMessage
    {
        public string Id { get; set; }
        [JsonProperty("dateTime")]
        public DateTime Date { get; set; }

        public string ClientId { get; set; }
        [JsonProperty("asset")]
        public string AssetId { get; set; }
        [JsonProperty("volume")]
        public string Amount { get; set; }
    }
}

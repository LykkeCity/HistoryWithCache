using System;
using Newtonsoft.Json;

namespace Lykke.Job.OperationsCache.Handlers.Models
{
    public class TransferQueueMessage
    {
        public string Id { get; set; }
        [JsonProperty("dateTime")]
        public DateTime Date { get; set; }

        public string FromClientId { get; set; }
        public string ToClientid { get; set; }

        [JsonProperty("asset")]
        public string AssetId { get; set; }
        [JsonProperty("volume")]
        public string Amount { get; set; }
    }
}

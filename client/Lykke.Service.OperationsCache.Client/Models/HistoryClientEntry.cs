using Lykke.Service.OperationsCache.AutorestClient.Models;

namespace Lykke.Service.OperationsCache.Client.Models
{
    public class HistoryClientEntry
    {
        public string Id { get; set; }
        public System.DateTime? DateTime { get; set; }
        public double? Amount { get; set; }
        public string Currency { get; set; }
        public string ClientId { get; set; }
        public string CustomData { get; set; }
        public string OpType { get; set; }
    }

    public static class Mapper
    {
        public static HistoryClientEntry FromApiModel(this HistoryEntry model)
        {
            return new HistoryClientEntry
            {
                Id = model.Id,
                Amount = model.Amount,
                ClientId = model.ClientId,
                Currency = model.Currency,
                CustomData = model.CustomData,
                DateTime = model.DateTime,
                OpType = model.OpType
            };
        }
    }
}

using Lykke.Service.OperationsCache.AutorestClient.Models;

namespace Lykke.Service.OperationsCache.Client.Models
{
    /// <summary>
    /// Represents history record
    /// </summary>
    public class HistoryClientEntry
    {
        /// <summary>
        /// Identifier of the record
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Timestamp of the operation
        /// </summary>
        public System.DateTime? DateTime { get; set; }
        /// <summary>
        /// Amount of the operation
        /// </summary>
        public double? Amount { get; set; }
        /// <summary>
        /// AssetId of the operations
        /// </summary>
        public string Currency { get; set; }
        /// <summary>
        /// The identifier of the client to whom the operation belongs
        /// </summary>
        public string ClientId { get; set; }
        /// <summary>
        /// The operations itself, serialized
        /// </summary>
        public string CustomData { get; set; }
        /// <summary>
        /// Operation type, possible values: CashInOut, CashOutAttempt, ClientTrade, TransferEvent, LimitTradeEvent 
        /// </summary>
        public string OpType { get; set; }
        /// <summary>
        /// The absolute or relative fee size
        /// </summary>
        public double FeeSize { get; set; }
        /// <summary>
        /// The type of the fee, possible values: Unknown, Absolute, Relative
        /// </summary>
        public string FeeType { get; set; }
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
                OpType = model.OpType,
                FeeSize = model.FeeSize,
                FeeType = model.FeeType
            };
        }
    }
}

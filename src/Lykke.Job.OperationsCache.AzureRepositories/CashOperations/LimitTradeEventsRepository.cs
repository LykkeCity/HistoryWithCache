using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AzureStorage;
using Core.CashOperations;
using Core.Offchain;

namespace AzureRepositories.CashOperations
{
    public class LimitTradeEventEntity : BaseEntity, ILimitTradeEvent
    {
        public DateTime CreatedDt { get; set; }
        public OrderType OrderType { get; set; }
        public double Volume { get; set; }
        public string AssetId { get; set; }
        public string AssetPair { get; set; }
        public double Price { get; set; }
        public OrderStatus Status { get; set; }
        public bool IsHidden { get; set; }
        public string ClientId { get; set; }
        public string Id { get; set; }
        public string OrderId { get; set; }

        public static string GeneratePartitionKey(string clientId)
        {
            return clientId;
        }
    }

    public class LimitTradeEventsRepository : ILimitTradeEventsRepository
    {
        private readonly INoSQLTableStorage<LimitTradeEventEntity> _storage;

        public LimitTradeEventsRepository(INoSQLTableStorage<LimitTradeEventEntity> storage)
        {
            _storage = storage;
        }

        public async Task<IEnumerable<ILimitTradeEvent>> GetEventsAsync(string clientId)
        {
            return await _storage.GetDataAsync(LimitTradeEventEntity.GeneratePartitionKey(clientId));
        }

        public async Task<IEnumerable<ILimitTradeEvent>> GetEventsAsync(string clientId, string orderId)
        {
            return await _storage.GetDataAsync(LimitTradeEventEntity.GeneratePartitionKey(clientId), entity => entity.OrderId == orderId);
        }
    }
}

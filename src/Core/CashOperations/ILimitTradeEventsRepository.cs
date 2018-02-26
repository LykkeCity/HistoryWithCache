using Core.Offchain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.CashOperations
{
    public interface ILimitTradeEvent
    {
        string ClientId { get; }
        string Id { get; }
        string OrderId { get; }
        DateTime CreatedDt { get; }
        OrderType OrderType { get; }
        double Volume { get; }
        string AssetId { get; }
        string AssetPair { get; }
        double Price { get; }
        OrderStatus Status { get; }
        bool IsHidden { get; }
    }

    public interface ILimitTradeEventsRepository
    {
        Task<IEnumerable<ILimitTradeEvent>> GetEventsAsync(string clientId);

        Task<IEnumerable<ILimitTradeEvent>> GetEventsAsync(string clientId, string orderId);
    }
}

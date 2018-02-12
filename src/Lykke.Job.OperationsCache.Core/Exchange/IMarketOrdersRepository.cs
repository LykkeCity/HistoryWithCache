using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Job.OperationsCache.Core.Exchange
{
    public enum OrderAction
    {
        Buy, Sell
    }

    public interface IOrderBase
    {
        string Id { get; }
        string ClientId { get; set; }
        DateTime CreatedAt { get; set; }
        double Volume { get; set; }
        double Price { get; set; }
        string AssetPairId { get; set; }
        string Status { get; set; }
        bool Straight { get; set; }
    }

    public interface ILimitOrder : IOrderBase
    {
        double RemainingVolume { get; set; }
        string MatchingId { get; set; }
    }

    public class MatchedOrder
    {
        public string Id { get; set; }
        public double Volume { get; set; }

        internal static MatchedOrder Create(IOrderBase orderBase, double volume)
        {
            return new MatchedOrder
            {
                Id = orderBase.Id,
                Volume = volume
            };
        }
    }

    public class MatchedLimitOrder : MatchedOrder
    {
        public double Price { get; set; }

        public static MatchedLimitOrder Create(ILimitOrder limitOrder, double volume)
        {
            return new MatchedLimitOrder
            {
                Price = limitOrder.Price,
                Id = limitOrder.Id,
                Volume = volume
            };
        }
    }

    public interface IMarketOrder : IOrderBase
    {
        DateTime MatchedAt { get; }
    }

    public class MarketOrder : IMarketOrder
    {
        public DateTime CreatedAt { get; set; }
        public string Id { get; set; }
        public string ClientId { get; set; }
        public string AssetPairId { get; set; }
        public OrderAction OrderAction { get; set; }
        public double Volume { get; set; }
        public double Price { get; set; }
        public string Status { get; set; }
        public bool Straight { get; set; }

        public DateTime MatchedAt { get; set; }
    }

    public interface IMarketOrdersRepository
    {
        Task CreateAsync(IMarketOrder marketOrder);
        Task<IMarketOrder> GetAsync(string orderId);
        Task<IMarketOrder> GetAsync(string clientId, string orderId);
        Task<IEnumerable<IMarketOrder>> GetOrdersAsync(string clientId);
        Task<IEnumerable<IMarketOrder>> GetOrdersAsync(IEnumerable<string> orderIds);
    }

    public static class BaseOrderExt
    {
        public const string Buy = "buy";
        public const string Sell = "sell";

        public static OrderAction OrderAction(this IOrderBase orderBase)
        {
            return orderBase.Volume > 0 ? Exchange.OrderAction.Buy : Exchange.OrderAction.Sell;
        }

        public static OrderAction? GetOrderAction(string actionWord)
        {
            if (actionWord.ToLower() == Buy)
                return Exchange.OrderAction.Buy;
            if (actionWord.ToLower() == Sell)
                return Exchange.OrderAction.Sell;

            return null;
        }

        public static OrderAction ViceVersa(this OrderAction orderAction)
        {
            if (orderAction == Exchange.OrderAction.Buy)
                return Exchange.OrderAction.Sell;
            return Exchange.OrderAction.Buy;
        }
    }
}

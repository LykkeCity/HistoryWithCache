using Common;
using Core.Exchange;
using Lykke.Job.OperationsCache.Models;
using Lykke.Service.Assets.Client.Models;
using System;

namespace Lykke.Job.OperationsCache.Services.OperationsHistory
{
    public static class Converter
    {
        public static ApiMarketOrder ConvertToApiModel(this IMarketOrder marketOrder, AssetPair assetPair, int accuracy)
        {
            var rate =
                (!marketOrder.Straight ? 1 / marketOrder.Price : marketOrder.Price).TruncateDecimalPlaces(
                    marketOrder.Straight ? assetPair.Accuracy : assetPair.InvertedAccuracy, marketOrder.OrderAction() == OrderAction.Buy);

            double converted = (rate * marketOrder.Volume).TruncateDecimalPlaces(accuracy, marketOrder.OrderAction() == OrderAction.Buy);

            var totalCost = marketOrder.OrderAction() == OrderAction.Sell ? marketOrder.Volume : converted;
            var volume = marketOrder.OrderAction() == OrderAction.Sell ? converted : marketOrder.Volume;

            return new ApiMarketOrder
            {
                Id = marketOrder.Id,
                OrderType = marketOrder.OrderAction().ToString(),
                AssetPair = marketOrder.AssetPairId,
                Volume = Math.Abs(volume),
                Comission = 0,
                Position = marketOrder.Volume,
                TotalCost = Math.Abs(totalCost),
                DateTime = marketOrder.CreatedAt.ToIsoDateTime(),
                Accuracy = assetPair.Accuracy,
                Price = rate,
                BaseAsset = marketOrder.Straight ? assetPair.BaseAssetId : assetPair.QuotingAssetId
            };
        }
    }
}

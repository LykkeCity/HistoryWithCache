using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.CashOperations;
using Lykke.Job.OperationsCache.Models;
using Lykke.Service.OperationsRepository.Core.CashOperations;
using Core.Exchange;
using Newtonsoft.Json.Linq;
using Common;
using Common.Log;
using Lykke.Service.Assets.Client.Models;
using Core;

namespace Lykke.Job.OperationsCache.Services.OperationsHistory
{
    public class OperationsHistoryRepoReader : IOperationsHistoryReader
    {
        private readonly ICashOperationsRepository _cashOperationsRepository;
        private readonly IClientTradesRepository _clientTradesRepository;
        private readonly ITransferEventsRepository _transferEventsRepository;
        private readonly ICashOutAttemptRepository _cashOutAttemptRepository;
        private readonly ILimitTradeEventsRepository _limitTradeEventsRepository;        
        private readonly IMarketOrdersRepository _marketOrdersRepository;
        private readonly CachedDataDictionary<string, AssetPair> _assetPairs;
        private readonly CachedAssetsDictionary _assets;
        private readonly ILog _log;

        private IReadOnlyDictionary<string, AssetPair> _assetPairValues;
        private IReadOnlyDictionary<string, Asset> _assetValues;
        private readonly TimeSpan _noAssetsExpirePeriod = TimeSpan.FromDays(1);
        private readonly Dictionary<string, DateTime> _noAssetPairs = new Dictionary<string, DateTime>();
        private readonly Dictionary<string, DateTime> _noAssets = new Dictionary<string, DateTime>();

        public OperationsHistoryRepoReader(
            ICashOperationsRepository cashOperationsRepository,
            IClientTradesRepository clientTradesRepository,
            ITransferEventsRepository transferEventsRepository,
            ICashOutAttemptRepository cashOutAttemptRepository,
            ILimitTradeEventsRepository limitTradeEventsRepository,            
            IMarketOrdersRepository marketOrdersRepository,
            CachedDataDictionary<string, AssetPair> assetPairs,
            CachedAssetsDictionary assets,
            ILog log)
        {
            _cashOperationsRepository = cashOperationsRepository ?? throw new ArgumentNullException(nameof(cashOperationsRepository));
            _clientTradesRepository = clientTradesRepository ?? throw new ArgumentNullException(nameof(clientTradesRepository));
            _transferEventsRepository = transferEventsRepository ?? throw new ArgumentNullException(nameof(transferEventsRepository));
            _cashOutAttemptRepository = cashOutAttemptRepository ?? throw new ArgumentNullException(nameof(cashOutAttemptRepository));
            _limitTradeEventsRepository = limitTradeEventsRepository ?? throw new ArgumentNullException(nameof(limitTradeEventsRepository));            
            _marketOrdersRepository = marketOrdersRepository ?? throw new ArgumentNullException(nameof(marketOrdersRepository));
            _assetPairs = assetPairs ?? throw new ArgumentNullException(nameof(assetPairs));
            _assets = assets ?? throw new ArgumentNullException(nameof(assets));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task<List<HistoryEntry>> GetHistory(string clientId)
        {
            var records = new List<HistoryEntry>();
            
            HistoryEntry[] cashOperations = null;
            HistoryEntry[] tradeOperations = null;
            HistoryEntry[] transferOperations = null;
            HistoryEntry[] cashOutAttemptOperations = null;
            HistoryEntry[] limitTradeEvents = null;
            Dictionary<string, IMarketOrder> marketOrders = null;

            var cashOperationsRead = _cashOperationsRepository.GetAsync(clientId);
            var tradeOperationsRead = _clientTradesRepository.GetAsync(clientId);
            var transferOperationsRead = _transferEventsRepository.GetAsync(clientId);            
            var cashOutAttemptsRead = _cashOutAttemptRepository.GetRequestsAsync(clientId);
            var limiTradeEventsRead = _limitTradeEventsRepository.GetEventsAsync(clientId);
            var marketOrdersRead = _marketOrdersRepository.GetOrdersAsync(clientId);

            await Task.WhenAll(
                cashOperationsRead.ContinueWith(t => cashOperations = t.Result.Select(RepoMapper.MapFrom).ToArray()),
                tradeOperationsRead.ContinueWith(t => tradeOperations = t.Result.Select(RepoMapper.MapFrom).ToArray()),
                transferOperationsRead.ContinueWith(t =>
                    transferOperations = t.Result.Select(RepoMapper.MapFrom).ToArray()),
                cashOutAttemptsRead.ContinueWith(t =>
                    cashOutAttemptOperations = t.Result.Select(RepoMapper.MapFrom).ToArray()),
                limiTradeEventsRead.ContinueWith(t => limitTradeEvents = t.Result.Select(RepoMapper.MapFrom).ToArray()),
                marketOrdersRead.ContinueWith(t => marketOrders = t.Result.Where(x => !string.IsNullOrEmpty(x.Id)).ToDictionary(x => x.Id))
            );

            records.AddRange(cashOperations);
            records.AddRange(tradeOperations);
            records.AddRange(transferOperations);
            records.AddRange(cashOutAttemptOperations);
            records.AddRange(limitTradeEvents);

            await AddMarketOrdersInfo(records, marketOrders);

            return records;
        }

        private async Task AddMarketOrdersInfo(List<HistoryEntry> records, Dictionary<string, IMarketOrder> marketOrders)
        {
            var clientTrades = records
                .Where(x => x.OpType == "ClientTrade");

            var clientTradesWithOrderId = clientTrades
                .Select(x =>
                {
                    var parsed = JObject.Parse(x.CustomData);
                    var marketOrderId = (string) parsed["MarketOrderId"];

                    return new
                    {
                        Trade = x,
                        MarketOrderId = marketOrderId
                    };
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.MarketOrderId));

            var assetPairs = await GetAssetPairs();

            var assets = await GetAssets();

            foreach (var tradeWithOrderId in clientTradesWithOrderId)
            {
                try
                {
                    var asset = assets.ContainsKey(tradeWithOrderId.Trade.Currency)
                        ? assets[tradeWithOrderId.Trade.Currency]
                        : null;

                    if (asset == null)
                    {
                        if (!_noAssets.ContainsKey(tradeWithOrderId.Trade.Currency))
                        {
                            _noAssets.Add(tradeWithOrderId.Trade.Currency, DateTime.UtcNow.Add(_noAssetsExpirePeriod));
                                
                            await _log.WriteWarningAsync(nameof(OperationsHistoryRepoReader), nameof(AddMarketOrdersInfo),
                                $"Unable to find asset in dictionary {tradeWithOrderId?.Trade?.Currency} for client {tradeWithOrderId?.Trade?.ClientId}");
                            continue;
                        }

                        if (_noAssets[tradeWithOrderId.Trade.Currency] > DateTime.UtcNow)
                            _noAssets.Remove(tradeWithOrderId.Trade.Currency);
                    }

                    if (marketOrders.TryGetValue(tradeWithOrderId.MarketOrderId, out var marketOrder))
                    {
                        var assetPair = assetPairs.ContainsKey(marketOrder.AssetPairId)
                            ? assetPairs[marketOrder.AssetPairId]
                            : null;

                        if (assetPair != null)
                        {
                            var parsed = JObject.Parse(tradeWithOrderId.Trade.CustomData);
                            var apiMarketOrder =
                                marketOrder.ConvertToApiModel(assetPair, asset.DisplayAccuracy ?? asset.Accuracy);
                            parsed.Add("MarketOrder", JObject.FromObject(apiMarketOrder));
                            tradeWithOrderId.Trade.CustomData = parsed.ToString();
                        }
                        else
                        {
                            if (!_noAssetPairs.ContainsKey(marketOrder.AssetPairId))
                            {
                                _noAssetPairs.Add(marketOrder.AssetPairId, DateTime.UtcNow.Add(_noAssetsExpirePeriod));
                                
                                await _log.WriteWarningAsync(nameof(OperationsHistoryRepoReader), nameof(AddMarketOrdersInfo),
                                $"Unable to find assetPair in dictionary {marketOrder?.AssetPairId} for client {tradeWithOrderId?.Trade?.ClientId}");
                            }
                            else if (_noAssetPairs[marketOrder.AssetPairId] > DateTime.UtcNow)
                                    _noAssetPairs.Remove(marketOrder.AssetPairId);
                        }
                    }
                }
                catch (Exception e)
                {
                    await _log.WriteErrorAsync(nameof(OperationsHistoryRepoReader), nameof(AddMarketOrdersInfo),
                        $"ClientId = {tradeWithOrderId?.Trade?.ClientId}", e);
                }
                
            }
        }

        private async Task<IReadOnlyDictionary<string, AssetPair>> GetAssetPairs()
        {
            if (_assetPairValues == null)
            {
                _assetPairValues = await _assetPairs.GetDictionaryAsync();
            }

            return _assetPairValues;
        }

        private async Task<IReadOnlyDictionary<string, Asset>> GetAssets()
        {
            if (_assetValues == null)
            {
                _assetValues = await _assets.GetDictionaryAsync();
            }

            return _assetValues;
        }
    }
}

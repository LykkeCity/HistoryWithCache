using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.CashOperations;
using Lykke.Job.OperationsCache.Models;
using Lykke.Service.OperationsRepository.Core.CashOperations;
using Core.BitCoin;
using Core.Exchange;
using Newtonsoft.Json.Linq;
using Common;
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
        private readonly IWalletCredentialsRepository _walletCredentialsRepository;
        private readonly IMarketOrdersRepository _marketOrdersRepository;
        private readonly CachedDataDictionary<string, AssetPair> _assetPairs;
        private CachedTradableAssetsDictionary _tradableAssets;

        public OperationsHistoryRepoReader(
            ICashOperationsRepository cashOperationsRepository,
            IClientTradesRepository clientTradesRepository,
            ITransferEventsRepository transferEventsRepository,
            ICashOutAttemptRepository cashOutAttemptRepository,
            ILimitTradeEventsRepository limitTradeEventsRepository,
            IWalletCredentialsRepository walletCredentialsRepository,
            IMarketOrdersRepository marketOrdersRepository,
            CachedDataDictionary<string, AssetPair> assetPairs,
            CachedTradableAssetsDictionary tradableAssets)
        {
            _cashOperationsRepository = cashOperationsRepository ?? throw new ArgumentNullException(nameof(cashOperationsRepository));
            _clientTradesRepository = clientTradesRepository ?? throw new ArgumentNullException(nameof(clientTradesRepository));
            _transferEventsRepository = transferEventsRepository ?? throw new ArgumentNullException(nameof(transferEventsRepository));
            _cashOutAttemptRepository = cashOutAttemptRepository ?? throw new ArgumentNullException(nameof(cashOutAttemptRepository));
            _limitTradeEventsRepository = limitTradeEventsRepository ?? throw new ArgumentNullException(nameof(limitTradeEventsRepository));
            _walletCredentialsRepository = walletCredentialsRepository ?? throw new ArgumentNullException(nameof(walletCredentialsRepository));
            _marketOrdersRepository = marketOrdersRepository ?? throw new ArgumentNullException(nameof(marketOrdersRepository));
            _assetPairs = assetPairs ?? throw new ArgumentNullException(nameof(assetPairs));
            _tradableAssets = tradableAssets ?? throw new ArgumentNullException(nameof(tradableAssets));
        }

        public async Task<List<HistoryEntry>> GetHistory(string clientId)
        {
            var records = new List<HistoryEntry>();
            var walletCreds = await _walletCredentialsRepository.GetAsync(clientId);
            var multisig = walletCreds?.MultiSig;

            if (!string.IsNullOrWhiteSpace(multisig))
            {
                records.AddRange((await _cashOperationsRepository.GetByMultisigAsync(multisig)).Select(RepoMapper.MapFrom));
                records.AddRange((await _clientTradesRepository.GetByMultisigAsync(multisig)).Select(RepoMapper.MapFrom));
                records.AddRange((await _transferEventsRepository.GetByMultisigAsync(multisig)).Select(RepoMapper.MapFrom));
            }
            else
            {
                records.AddRange((await _cashOperationsRepository.GetAsync(clientId)).Select(RepoMapper.MapFrom));
                records.AddRange((await _clientTradesRepository.GetAsync(clientId)).Select(RepoMapper.MapFrom));
                records.AddRange((await _transferEventsRepository.GetAsync(clientId)).Select(RepoMapper.MapFrom));
            }

            records.AddRange((await _cashOutAttemptRepository.GetRequestsAsync(clientId)).Select(RepoMapper.MapFrom));
            records.AddRange((await _limitTradeEventsRepository.GetEventsAsync(clientId)).Select(RepoMapper.MapFrom));

            await AddMarketOrdersInfo(records);

            return records;
        }

        private async Task AddMarketOrdersInfo(List<HistoryEntry> records)
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

            var assetPairs = await _assetPairs.GetDictionaryAsync();

            var assets = await _tradableAssets.GetDictionaryAsync();

            var orderIds = clientTradesWithOrderId.Select(x => x.MarketOrderId).Distinct();

            var ordersDict = (await _marketOrdersRepository.GetOrdersAsync(orderIds)).ToDictionary(x => x.Id);

            foreach (var tradeWithOrderId in clientTradesWithOrderId)
            {
                var asset = assets[tradeWithOrderId.Trade.Currency];

                if (asset == null)
                    continue;

                if (ordersDict.TryGetValue(tradeWithOrderId.MarketOrderId, out var marketOrder))
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
                }
            }
        }
    }
}

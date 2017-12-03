using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.OperationsCache.Models;
using Lykke.Job.OperationsCache.Services.OperationsHistory;

namespace Lykke.Job.OperationsCache.Services.InMemoryCache
{
    public class InMemoryCache : IHistoryCache
    {
        private readonly ConcurrentDictionary<string, CacheModel> _storage;
        private readonly ILog _log;
        private readonly IOperationsHistoryReader _operationsHistoryReader;
        private readonly int _valuesPerPage = 100; // todo: use settings

        public InMemoryCache(ILog log, IOperationsHistoryReader operationsHistoryReader)
        {
            _storage = new ConcurrentDictionary<string, CacheModel>();
            _log = log;
            _operationsHistoryReader = operationsHistoryReader ?? throw new ArgumentNullException(nameof(operationsHistoryReader));
        }

        public async Task<IEnumerable<HistoryEntry>> GetAllPagedAsync(string clientId, int page)
        {
            return await InternalGetAllAsync(
                clientId,
                GetTopValueForPagedApi(),
                GetSkipValueForPagedApi(page));
        }

        public async Task<IEnumerable<HistoryEntry>> GetAllAsync(string clientId, int top, int skip)
        {
            return await InternalGetAllAsync(clientId, top, skip);
        }

        public async Task<IEnumerable<HistoryEntry>> GetAllPagedAsync(string clientId, string assetId, string operationType, int page)
        {
            return await InternalGetAllAsync(
                clientId,
                assetId,
                operationType,
                GetTopValueForPagedApi(),
                GetSkipValueForPagedApi(page));
        }

        public async Task<IEnumerable<HistoryEntry>> GetAllAsync(string clientId, string assetId, string operationType, int top, int skip)
        {
            return await InternalGetAllAsync(clientId, assetId, operationType, top, skip);
        }

        public async Task<IEnumerable<HistoryEntry>> GetAllByOpTypePagedAsync(string clientId, string operationType, int page)
        {
            return await InternalGetAllByOpTypeAsync(
                clientId,
                operationType,
                GetTopValueForPagedApi(),
                GetSkipValueForPagedApi(page));
        }

        public async Task<IEnumerable<HistoryEntry>> GetAllByOpTypeAsync(string clientId, string operationType, int top, int skip)
        {
            return await InternalGetAllByOpTypeAsync(clientId, operationType, top, skip);
        }

        public async Task<IEnumerable<HistoryEntry>> GetAllByAssetPagedAsync(string clientId, string assetId, int page)
        {
            return await InternalGetAllByAssetAsync(
                clientId,
                assetId,
                GetTopValueForPagedApi(),
                GetSkipValueForPagedApi(page));
        }

        public async Task<IEnumerable<HistoryEntry>> GetAllByAssetAsync(string clientId, string assetId, int top, int skip)
        {
            return await InternalGetAllByAssetAsync(clientId, assetId, top, skip);
        }

        private async Task<IEnumerable<HistoryEntry>> InternalGetAllAsync(string clientId, int top, int skip)
        {
            var clientRecords = await GetRecordsByClient(clientId);

            var pagedResult = clientRecords
                .Skip(skip)
                .Take(top)
                .ToList();

            return pagedResult;
        }

        private async Task<IEnumerable<HistoryEntry>> InternalGetAllAsync(string clientId, string assetId, string operationType,
            int top, int skip)
        {
            var clientRecords = await GetRecordsByClient(clientId);

            var pagedResult = clientRecords
                .Where(r => r.Currency == assetId && r.OpType == operationType)
                .Skip(skip)
                .Take(top)
                .ToList();

            return pagedResult;
        }

        private async Task<IEnumerable<HistoryEntry>> InternalGetAllByOpTypeAsync(string clientId, string operationType, int top, int skip)
        {
            var clientRecords = await GetRecordsByClient(clientId);

            var pagedResult = clientRecords
                .Where(r => r.OpType == operationType)
                .Skip(skip)
                .Take(top)
                .ToList();

            return pagedResult;
        }

        private async Task<IEnumerable<HistoryEntry>> InternalGetAllByAssetAsync(string clientId, string assetId, int top, int skip)
        {
            var clientRecords = await GetRecordsByClient(clientId);

            var pagedResult = clientRecords
                .Where(r => r.Currency == assetId)
                .Skip(skip)
                .Take(top)
                .ToList();

            return pagedResult;
        }

        private int GetSkipValueForPagedApi(int page)
        {
            return (page - 1) * _valuesPerPage;
        }

        private int GetTopValueForPagedApi()
        {
            return _valuesPerPage;
        }

        public async Task<IEnumerable<HistoryEntry>> GetRecordsByClient(string clientId)
        {
            if (_storage.TryGetValue(clientId, out CacheModel cachedValue))
            {
                return cachedValue.Records.Values;
            }

            var newCachedValue = await Load(clientId);

            return newCachedValue == null ? new List<HistoryEntry>() : newCachedValue.Records.Values;
        }

        public void AddOrUpdate(HistoryEntry item)
        {
            if (_storage.TryGetValue(item.ClientId, out CacheModel cachedCollection))
            {
                cachedCollection.Records.AddOrUpdate(item.Id, item, (key, oldValue) => item);

                return;
            }

            _log?.WriteWarningAsync(nameof(InMemoryCache), nameof(AddOrUpdate), $"clientId = {item.ClientId}",
                "No cache for clientId, new item will be ignored");
        }

        public async Task WarmUp(string clientId, bool force = false)
        {
            if (force || !_storage.ContainsKey(clientId))
            {
                await Load(clientId);
            }
        }

        private async Task<CacheModel> Load(string clientId)
        {
            var records = await _operationsHistoryReader.GetHistory(clientId);

            var cacheModel = new CacheModel
            {
                Records = new ConcurrentDictionary<string, HistoryEntry>(
                    records
                        .OrderBy(r => r.DateTime)
                        .Select(x => new KeyValuePair<string, HistoryEntry>(x.Id ?? Guid.NewGuid().ToString(), x)))
            };

            return _storage.AddOrUpdate(clientId, cacheModel, (key, oldValue) => cacheModel);
        }
    }
}

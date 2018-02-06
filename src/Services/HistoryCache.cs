using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Core.Domain;
using Core.Services;

namespace Services
{
    public class HistoryCache : IHistoryCache
    {
        private readonly ILog _log;
        private readonly IStorage _storage;
        private readonly IOperationsHistoryReader _operationsHistoryReader;
        private readonly int _valuesPerPage;
        private readonly int _maxHistoryLengthPerClient;
        private readonly int _saveHistoryLengthPerClient;

        public HistoryCache(ILog log, IStorage storage, IOperationsHistoryReader operationsHistoryReader, int valuesPerPage, int maxHistoryLengthPerClient, int saveHistoryLengthPerClient)
        {
            _log = log;
            _storage = storage;
            _operationsHistoryReader = operationsHistoryReader ?? throw new ArgumentNullException(nameof(operationsHistoryReader));
            _valuesPerPage = valuesPerPage;
            _maxHistoryLengthPerClient = maxHistoryLengthPerClient;
            _saveHistoryLengthPerClient = saveHistoryLengthPerClient;
        }

        public async Task<IEnumerable<HistoryEntry>> GetAllPagedAsync(string clientId, int page)
        {
            return await InternalGetAllAsync(
                clientId,
                GetTopValueForPagedApi(),
                GetSkipValueForPagedApi(page));
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
            var history = await _storage.Get(clientId);

            if (history != null)
                return history;

            var newCachedValue = await Load(clientId);

            return newCachedValue?.Records ?? Enumerable.Empty<HistoryEntry>();
        }

        public async Task WarmUp(string clientId, bool force = false)
        {
            if (force || _storage.Get(clientId) == null)
            {
                await Load(clientId);
            }
        }

        private async Task<CacheModel> Load(string clientId)
        {
            var records = await _operationsHistoryReader.GetHistory(clientId);
            if (records.Count > _maxHistoryLengthPerClient)
            {
                await _log.WriteWarningAsync(nameof(HistoryCache), nameof(Load), $"ClientId: {clientId}", $"Records: {records.Count}");
                return new CacheModel();
            }

            var cacheModel = new CacheModel
            {
                Records = records.OrderByDescending(r => r.DateTime).Take(_saveHistoryLengthPerClient).ToList()
            };

            await _storage.Set(clientId, cacheModel);

            return cacheModel;
        }
    }
}

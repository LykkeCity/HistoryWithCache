using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.OperationsCache.Models;
using Lykke.Job.OperationsCache.Services.OperationsHistory;

namespace Lykke.Job.OperationsCache.Services.InMemoryCache
{
    public class HistoryCache : IHistoryCache
    {        
        private readonly ILog _log;
        private readonly IStorage _storage;
        private readonly IList<string> _excludeList;
        private readonly IOperationsHistoryReader _operationsHistoryReader;
        private readonly int _valuesPerPage;

        public HistoryCache(ILog log, IStorage storage, IList<string> excludeList, IOperationsHistoryReader operationsHistoryReader, int valuesPerPage)
        {            
            _log = log;
            _storage = storage;
            _excludeList = excludeList;
            _operationsHistoryReader = operationsHistoryReader ?? throw new ArgumentNullException(nameof(operationsHistoryReader));
            _valuesPerPage = valuesPerPage;
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

            return newCachedValue?.Records.Values.Select(v => v) ?? Enumerable.Empty<HistoryEntry>();
        }
        
        public async Task WarmUp(string clientId, bool force = false)
        {
            if (_excludeList.Contains(clientId))
                return;

            if (force || _storage.Get(clientId) == null)
            {
                await Load(clientId);
            }
        }

        private async Task<CacheModel> Load(string clientId)
        {            
            var records = await _operationsHistoryReader.GetHistory(clientId);
         
            var cacheModel = new CacheModel
            {
                Records = new Dictionary<string, HistoryEntry>(
                    records
                        .OrderByDescending(r => r.DateTime)
                        .ToDictionary(k => k.Id ?? Guid.NewGuid().ToString(), v => v))
            };

            await _storage.Set(clientId, cacheModel);

            return cacheModel;
        } 
    }
}

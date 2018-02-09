using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.OperationsCache.Core.Domain;
using Lykke.Job.OperationsCache.Core.Services;

namespace Lykke.Job.OperationsCache.Services
{
    public class HistoryCache : IHistoryCache
    {
        private readonly ILog _log;
        private readonly IStorage _storage;
        private readonly IOperationsHistoryReader _operationsHistoryReader;
        private readonly int _maxHistoryLengthPerClient;
        private readonly int _saveHistoryLengthPerClient;

        public HistoryCache(ILog log, IStorage storage, IOperationsHistoryReader operationsHistoryReader, int maxHistoryLengthPerClient, int saveHistoryLengthPerClient)
        {
            _log = log;
            _storage = storage;
            _operationsHistoryReader = operationsHistoryReader ?? throw new ArgumentNullException(nameof(operationsHistoryReader));
            _maxHistoryLengthPerClient = maxHistoryLengthPerClient;
            _saveHistoryLengthPerClient = saveHistoryLengthPerClient;
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
                await _log.WriteWarningAsync(nameof(HistoryCache), nameof(Load), $"ClientId: {clientId}", $"Records: {records.Count}, taking {_saveHistoryLengthPerClient} records");
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

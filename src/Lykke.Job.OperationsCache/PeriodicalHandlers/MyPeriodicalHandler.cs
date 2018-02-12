using System;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using System.Collections.Generic;
using Lykke.Job.OperationsCache.Core;
using Lykke.Job.OperationsCache.Core.Services;

namespace Lykke.Job.OperationsCache.PeriodicalHandlers
{
    public class MyPeriodicalHandler : TimerPeriod
    {
        private readonly IList<string> _excludeList;
        private readonly ILog _log;
        private readonly IHistoryCache _historyCache;
        private readonly CachedSessionsDictionary _cachedSessionsDictionary;
        private static bool _inProcess;
        
        public MyPeriodicalHandler(
            CachedSessionsDictionary cachedSessionsDictionary,
            IHistoryCache historyCache,
            ILog log,
            TimeSpan expirationPeriod,
            IList<string> excludeList) :
            base(nameof(MyPeriodicalHandler), (int)expirationPeriod.TotalMilliseconds, log)
        {
            _excludeList = excludeList;
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _cachedSessionsDictionary = cachedSessionsDictionary ?? throw new ArgumentNullException(nameof(cachedSessionsDictionary));
            _historyCache = historyCache ?? throw new ArgumentNullException(nameof(historyCache));            
        }

        public override async Task Execute()
        {
            if (_inProcess)
                return;

            try
            {
                _inProcess = true;

                var timestamp = DateTime.UtcNow;
                var clientsIds = (await _cachedSessionsDictionary.Values()).Where(id => !_excludeList.Contains(id)).ToList();

                await _log.WriteInfoAsync(GetComponentName(), "Updating cache", $"Processing {clientsIds.Count} active clients.");
                foreach (var clientId in clientsIds)
                {
                    await _historyCache.WarmUp(clientId, true).ConfigureAwait(false);
                }

                await _log.WriteInfoAsync(GetComponentName(), "Updating cache", $"Processed in {(DateTime.UtcNow - timestamp).TotalSeconds} seconds.");
            }
            finally
            {

                _inProcess = false;
            }
        }
    }
}

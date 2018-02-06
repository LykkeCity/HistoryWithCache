using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Core;
using Core.Services;

namespace Services
{
    public class DelayWarmUp : IDelayWarmUp, IDisposable
    {
        private readonly IHistoryCache _historyCache;
        private readonly TimeSpan _delayPeriod;
        private readonly IList<string> _excludeList;
        private readonly CachedSessionsDictionary _sessions;
        private readonly Subject<string> _subject = new Subject<string>();

        public DelayWarmUp(
            IHistoryCache historyCache,
            TimeSpan delayPeriod,
            IList<string> excludeList,
            CachedSessionsDictionary sessions)
        {
            _historyCache = historyCache;
            _delayPeriod = delayPeriod;
            _excludeList = excludeList;
            _sessions = sessions;
            _subject.Delay(delayPeriod).Subscribe(async clientId => await WarmUp(clientId));
        }

        public void Dispose()
        {
            _subject?.Dispose();
        }

        public async Task OnNewOperation(string clientId)
        {
            if (_excludeList.Contains(clientId))
                return;

            var activeSessions = await _sessions.GetDictionaryAsync();
            if (!activeSessions.ContainsKey(clientId))
                return;

            _subject.OnNext(clientId);
        }

        private async Task WarmUp(string clientId)
        {
            await _historyCache.WarmUp(clientId, true);
        }
    }
}

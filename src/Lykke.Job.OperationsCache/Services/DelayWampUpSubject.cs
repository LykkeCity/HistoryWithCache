using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Lykke.Job.OperationsCache.Services
{
    public class DelayWampUpSubject : IDisposable, IDelayWampUpSubject
    {
        private readonly IHistoryCache _historyCache;
        private readonly TimeSpan _delayPeriod;
        private readonly IList<string> _excludeList;
        private readonly Subject<string> _subject = new Subject<string>();

        public DelayWampUpSubject(IHistoryCache historyCache, TimeSpan delayPeriod, IList<string> excludeList)
        {
            _historyCache = historyCache;
            _delayPeriod = delayPeriod;
            _excludeList = excludeList;
            _subject.Delay(delayPeriod).ObserveOn(new EventLoopScheduler()).Subscribe(async clientId => await WarmUp(clientId));
        }


        public void OnNewOperation(string clientId)
        {
            _subject.OnNext(clientId);            
        }

        private async Task WarmUp(string clientId)
        {
            if (_excludeList.Contains(clientId))
                return;

            await _historyCache.WarmUp(clientId, true);
        }

        public void Dispose()
        {
            _subject?.Dispose();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Lykke.Job.OperationsCache.Services
{
    public class DelayWampUpSubject : IDisposable, IDelayWampUpSubject
    {
        private readonly IHistoryCache _historyCache;        
        private readonly Subject<string> _subject = new Subject<string>();

        public DelayWampUpSubject(IHistoryCache historyCache, TimeSpan delayPeriod)
        {
            _historyCache = historyCache;            
            _subject.Delay(delayPeriod).Subscribe(async clientId => await WarmUp(clientId));
        }


        public void OnNewOperation(string clientId)
        {
            _subject.OnNext(clientId);            
        }

        private async Task WarmUp(string clientId)
        {
            await _historyCache.WarmUp(clientId, true);
        }

        public void Dispose()
        {
            _subject?.Dispose();
        }
    }
}

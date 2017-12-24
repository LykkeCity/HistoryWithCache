using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Job.OperationsCache.Services
{
    public class DelayWarmUp : IDelayWarmUp
    {
        private readonly IHistoryCache _historyCache;
        private readonly TimeSpan _delayPeriod;
        private readonly IList<string> _excludeList;

        public DelayWarmUp(IHistoryCache historyCache, TimeSpan delayPeriod, IList<string> excludeList)
        {
            _historyCache = historyCache;
            _delayPeriod = delayPeriod;
            _excludeList = excludeList;
        }

        public async Task OnNewOperation(string clientId)
        {
            if (_excludeList.Contains(clientId))
                return;

            await Task.Delay(_delayPeriod).ContinueWith(_ => WarmUp(clientId));
        }

        private async Task WarmUp(string clientId)
        {
            await _historyCache.WarmUp(clientId, true);
        }
    }
}

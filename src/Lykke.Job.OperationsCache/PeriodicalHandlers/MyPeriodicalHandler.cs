using System;
using System.Collections.Async;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Job.OperationsCache.Services;

namespace Lykke.Job.OperationsCache.PeriodicalHandlers
{
    public class MyPeriodicalHandler : TimerPeriod
    {
        private readonly IHistoryCache _historyCache;
        private readonly ClientSessionsRepository _clientSessionsRepository;
        private static bool _inProcess;

        public MyPeriodicalHandler(
            ClientSessionsRepository clientSessionsRepository,
            IHistoryCache historyCache,
            ILog log,
            TimeSpan expirationPeriod) :
            base(nameof(MyPeriodicalHandler), (int)expirationPeriod.TotalMilliseconds, log)
        {
            _clientSessionsRepository = clientSessionsRepository ?? throw new ArgumentNullException(nameof(_clientSessionsRepository));
            _historyCache = historyCache ?? throw new ArgumentNullException(nameof(historyCache));
        }

        public override async Task Execute()
        {
            if (_inProcess)
                return;

            try
            {
                _inProcess = true;

                var clientsIds = await _clientSessionsRepository.GetClientsIds();

                await clientsIds.ParallelForEachAsync(async clientId =>
                {
                    await _historyCache.WarmUp(clientId).ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
            finally
            {

                _inProcess = false;
            }
        }
    }
}

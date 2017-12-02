using System;
using System.Threading.Tasks;
using Common;
using Common.Log;

namespace Lykke.Job.OperationsCache.PeriodicalHandlers
{
    public class MyPeriodicalHandler : TimerPeriod
    {
        private static bool _inProcess = false;

        public MyPeriodicalHandler(ILog log) :
            // TODO: Sometimes, it is enough to hardcode the period right here, but sometimes it's better to move it to the settings.
            // Choose the simplest and sufficient solution
            base(nameof(MyPeriodicalHandler), (int)TimeSpan.FromSeconds(10).TotalMilliseconds, log)
        {
        }

        public override async Task Execute()
        {
            // TODO: Orchestrate execution flow here and delegate actual business logic implementation to services layer
            // Do not implement actual business logic here
            if (_inProcess)
                return;

            try
            {
                _inProcess = true;
                await Task.CompletedTask;
            }
            finally
            {

                _inProcess = false;
            }
        }
    }
}

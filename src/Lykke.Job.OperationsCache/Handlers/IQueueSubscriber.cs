using System;

namespace Lykke.Job.OperationsCache.Handlers
{
    public interface IQueueSubscriber : IDisposable
    {
        void Start();
        void Stop();
    }
}
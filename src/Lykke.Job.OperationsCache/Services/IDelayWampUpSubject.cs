namespace Lykke.Job.OperationsCache.Services
{
    public interface IDelayWampUpSubject
    {
        void OnNewOperation(string clientId);
    }
}

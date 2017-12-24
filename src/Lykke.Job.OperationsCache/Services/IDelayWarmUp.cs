using System.Threading.Tasks;

namespace Lykke.Job.OperationsCache.Services
{
    public interface IDelayWarmUp
    {
        Task OnNewOperation(string clientId);
    }
}

using System.Threading.Tasks;

namespace Lykke.Job.OperationsCache.Core.Services
{
    public interface IDelayWarmUp
    {
        Task OnNewOperation(string clientId);
    }
}

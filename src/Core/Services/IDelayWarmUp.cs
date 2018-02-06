using System.Threading.Tasks;

namespace Core.Services
{
    public interface IDelayWarmUp
    {
        Task OnNewOperation(string clientId);
    }
}

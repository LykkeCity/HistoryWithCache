using System.Threading.Tasks;

namespace Lykke.Job.OperationsCache.Core.Services
{
    public interface IShutdownManager
    {
        Task StopAsync();
    }
}
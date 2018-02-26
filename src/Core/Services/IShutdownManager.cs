using System.Threading.Tasks;

namespace Core.Services
{
    public interface IShutdownManager
    {
        Task StopAsync();
    }
}
using System.Threading.Tasks;
using Lykke.Service.OperationsCache.Client.Models;
using System.Collections.Generic;

namespace Lykke.Service.OperationsCache.Client
{
    public interface IOperationsCacheClient
    {
        Task<IEnumerable<HistoryClientEntry>>GetHistoryByClientId(string clientId);
    }
}

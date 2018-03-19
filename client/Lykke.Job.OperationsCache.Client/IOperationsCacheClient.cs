using System.Threading.Tasks;
using Lykke.Job.OperationsCache.Client.Models;
using System.Collections.Generic;

namespace Lykke.Job.OperationsCache.Client
{
    public interface IOperationsCacheClient
    {
        Task<IEnumerable<HistoryClientEntry>>GetHistoryByClientId(string clientId);
        Task RemoveCashoutIfExists(string clientId, string operationId);
    }
}

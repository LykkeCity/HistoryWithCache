using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Job.OperationsCache.Core.Domain;

namespace Lykke.Job.OperationsCache.Core.Services
{
    public interface IOperationsHistoryReader
    {
        Task<List<HistoryEntry>> GetHistory(string clientId);
    }
}

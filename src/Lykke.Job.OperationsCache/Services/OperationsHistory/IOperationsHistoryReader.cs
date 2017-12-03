using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Job.OperationsCache.Models;

namespace Lykke.Job.OperationsCache.Services.OperationsHistory
{
    public interface IOperationsHistoryReader
    {
        Task<List<HistoryEntry>> GetHistory(string clientId);
    }
}

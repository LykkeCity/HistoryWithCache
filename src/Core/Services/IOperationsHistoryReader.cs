using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Domain;

namespace Core.Services
{
    public interface IOperationsHistoryReader
    {
        Task<List<HistoryEntry>> GetHistory(string clientId);
    }
}

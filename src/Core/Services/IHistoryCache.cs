﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Domain;

namespace Core.Services
{
    public interface IHistoryCache
    {
        Task<IEnumerable<HistoryEntry>> GetAllPagedAsync(string clientId, int page);

        Task<IEnumerable<HistoryEntry>> GetRecordsByClient(string clientId);
        
        Task WarmUp(string clientId, bool force = false);
    }
}

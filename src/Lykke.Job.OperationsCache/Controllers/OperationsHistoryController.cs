using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Lykke.Job.OperationsCache.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Job.OperationsCache.Controllers
{
    [Route("api/[controller]")]
    public class OperationsHistoryController : Controller
    {
        private readonly IHistoryCache _historyCache;

        public OperationsHistoryController(IHistoryCache historyCache)
        {
            _historyCache = historyCache ?? throw new ArgumentNullException(nameof(historyCache));
        }

        /// <summary>
        /// Get operations history.
        /// </summary>
        [HttpGet]
        [SwaggerOperation("GetHistory")]
        [ProducesResponseType(typeof(IEnumerable<Models.HistoryEntry>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetHistory(string clientId)
        {
            var history = await _historyCache.GetAllPagedAsync(clientId, 1);
            return Ok(history);
        }
    }
}

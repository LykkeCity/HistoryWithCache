using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Lykke.Job.OperationsCache.Services;
using Lykke.Service.OperationsRepository.Core.CashOperations;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Job.OperationsCache.Controllers
{
    [Route("api/[controller]")]
    public class OperationsHistoryController : Controller
    {
        private readonly IHistoryCache _historyCache;
        private readonly ICashOperationsRepository _cashOperationsRepository;

        public OperationsHistoryController(
            ICashOperationsRepository cashOperationsRepository,
            IHistoryCache historyCache)
        {
            _historyCache = historyCache ?? throw new ArgumentNullException(nameof(historyCache));
            _cashOperationsRepository = cashOperationsRepository ?? throw new ArgumentNullException(nameof(cashOperationsRepository));
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
        
        [HttpDelete]
        [SwaggerOperation("DeleteCashOperation")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteCashOpertaionIfExists(string clientId, string operationId)
        {
            await _cashOperationsRepository.RemoveIfExistsAsync(clientId, operationId);
            
            await _historyCache.WarmUp(clientId, true);

            return Ok();
        }
    }
}

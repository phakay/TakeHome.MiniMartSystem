using Microsoft.AspNetCore.Mvc;
using MiniMart.Application.Contracts;

namespace MiniMart.API.Controllers
{
    [ApiController]
    [Route("api/alerts")]
    public class StockAlertsController : ControllerBase
    {
        private readonly IStockAlertService _service;
        public StockAlertsController(IStockAlertService svc)
        {
            _service = svc;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var response = await _service.GetStockAlertsAsync();
            return Ok(response);
        }
    }
}

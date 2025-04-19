using Microsoft.AspNetCore.Mvc;
using MiniMart.API.ActionFilters;
using MiniMart.Application.Contracts;
using MiniMart.Application.Models;

namespace MiniMart.API.Controllers
{
    [ApiController]
    [Route("api/webhook")]
    public class WebhookController : ControllerBase
    {
        private readonly ILogger<WebhookController> _logger;
        private readonly IWebhookService _webhookService;

        public WebhookController(ILogger<WebhookController> logger, IWebhookService webhookService)
        {
            _logger = logger;
            _webhookService = webhookService;
        }

        [HttpPost]
        [WebhookActionFilter]
        [Consumes("application/json")]
        public async Task<IActionResult> ReceiveWebhook([FromBody] CallbackRequest request)
        {
            var req = new WebhookRequest
            { };

            await _webhookService.ProcessWebhookTransaction(req);

            return Ok(new WebhookResponse { Message = "Processed Payload" });
        }
    }
}

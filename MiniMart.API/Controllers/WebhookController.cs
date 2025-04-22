using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MiniMart.API.ActionFilters;
using MiniMart.Application.Contracts;
using MiniMart.Application.Models;

namespace MiniMart.API.Controllers
{
    [ApiController]
    [Route("api/webhook")]
    public class WebhookController : BaseController
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
            _logger.LogInformation("Received Webhook request");
            try
            {
                var reqJson = JsonSerializer.Serialize(request);
                _logger.LogInformation($"Received {reqJson}");
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Serialization operation failed");
            }
           
            var req = new WebhookRequest
            {
                RefId = request.TraceId,
                IsSuccess = request.TransactionResponseCode == Constants.Successful
            };

            await _webhookService.ProcessWebhookTransactionAsync(req);

            return CreateCustomResult(HttpStatusCode.OK, new WebhookResponse { Message = "Processed Payload" });
        }
    }
}

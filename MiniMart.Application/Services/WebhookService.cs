using MiniMart.Application.Contracts;
using MiniMart.Application.Models;

namespace MiniMart.Application.Services
{
    public class WebhookService : IWebhookService
    {
        private readonly IPurchaseOrderService _purchaseOrderService;
        public WebhookService(IPurchaseOrderService purchaseOrderService)
        {
            _purchaseOrderService = purchaseOrderService;
        }

        public async Task ProcessWebhookTransactionAsync(WebhookRequest request)
        {
            await _purchaseOrderService.ProcessOrderTransactionStatus(request.RefId, request.IsSuccess);
        }
    }
}

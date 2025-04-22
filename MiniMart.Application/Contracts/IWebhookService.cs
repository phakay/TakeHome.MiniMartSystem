using MiniMart.Application.Models;

namespace MiniMart.Application.Contracts
{
    public interface IWebhookService
    {
        public Task ProcessWebhookTransactionAsync(WebhookRequest request);
    }
}

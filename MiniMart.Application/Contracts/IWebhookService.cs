using MiniMart.Application.Models;

namespace MiniMart.Application.Contracts
{
    public interface IWebhookService
    {
        public Task ProcessWebhookTransaction(WebhookRequest request);
    }
}

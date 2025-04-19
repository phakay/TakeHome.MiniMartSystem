using MiniMart.Application.Models;
using MiniMart.Domain.Models;

namespace MiniMart.Application.Contracts
{
    public interface IPurchaseOrderService
    {
        public Task<PurchaseResponse> ProcessPurchaseOrderAsync(PurchaseRequest request);

        public Task<IEnumerable<PurchaseOrder>> GetPurchaseOrdersAsync();

        public Task<PurchaseOrderStatusResponse?> VerifyOrderStatusAsync(string referenceId);

        public Task ProcessOrderTransactionStatus(string refId, bool isSuccessful);
    }
}

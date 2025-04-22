using MiniMart.Application.Models;
using MiniMart.Domain.Models;

namespace MiniMart.Application.Contracts
{
    public interface IPurchaseOrderService
    {
        public Task<ServiceResponse<PurchaseResponse>> ProcessPurchaseOrderAsync(PurchaseRequest request);

        public Task<IEnumerable<PurchaseOrder>> GetPurchaseOrdersAsync();

        public Task<PurchaseOrderStatusResponse?> VerifyOrderStatusAsync(string referenceId);

        public Task<ServiceResponse> ProcessOrderTransactionStatus(string refId, bool isSuccessful);
    }
}

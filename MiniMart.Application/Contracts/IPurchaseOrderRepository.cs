using MiniMart.Domain.Models;

namespace MiniMart.Application.Contracts
{
    public interface IPurchaseOrderRepository : IRepository<PurchaseOrder>
    {
        Task<PurchaseOrder?> GetByReferenceIdAsync(string referenceId);
    }
}

using MiniMart.Domain.Models;

namespace MiniMart.Application.Contracts
{
    public interface IProductInventoryRepository : IRepository<ProductInventory> 
    {
        Task<ProductInventory?> GetByProductIdAsync(int productId);
    }
}

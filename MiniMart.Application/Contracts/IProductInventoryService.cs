using System.Linq.Expressions;
using MiniMart.Domain.Models;

namespace MiniMart.Application.Contracts
{
    public interface IProductInventoryService
    {
        Task<ProductInventory?> GetProductInventoryByProductIdAsync(int productId);
        Task<IEnumerable<ProductInventory>> GetAllProductInventoriesAsync();
        Task<IEnumerable<ProductInventory>> GetAvailableProductsInStockAsync();
        Task AddQuantityToInventoryAsync(int productId, int quantity);
        Task RemoveQuantityFromInventory(int productId, int quantity);
    }
}

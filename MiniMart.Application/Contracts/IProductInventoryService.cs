using System.Linq.Expressions;
using MiniMart.Application.Models;
using MiniMart.Domain.Models;

namespace MiniMart.Application.Contracts
{
    public interface IProductInventoryService
    {
        Task<ProductInventory?> GetProductInventoryByProductIdAsync(int productId);
        Task<IEnumerable<ProductInventory>> GetAllProductInventoriesAsync();
        Task<IEnumerable<ProductInventory>> GetAvailableProductsInStockAsync();
        Task<ServiceResponse> AddQuantityToInventoryAsync(int productId, int quantity);
        Task<ServiceResponse> RemoveQuantityFromInventory(int productId, int quantity);
    }
}

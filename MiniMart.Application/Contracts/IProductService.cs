using MiniMart.Domain.Models;

namespace MiniMart.Application.Contracts
{
    public interface IProductService
    {
        Task<Product?> GetProductByIdAsync(int id);
        Task<bool> DoesProductIdExistsAsync(int id);
        Task<bool> DoesProductNameExistsAsync(string name);
        Task<IEnumerable<Product>> GetProductsAsync();
        Task AddProductAsync(Product item);
        Task UpdateProductAsync(Product item);
        Task RemoveProductAsync(int id);
    }
}
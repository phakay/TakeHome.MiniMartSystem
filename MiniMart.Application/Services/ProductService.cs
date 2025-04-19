using MiniMart.Application.Contracts;
using MiniMart.Domain.Models;

namespace MiniMart.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        private readonly IProductInventoryRepository _inventoryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IProductRepository repository, IUnitOfWork unitOfWork, IProductInventoryRepository inventoryRepository)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _inventoryRepository = inventoryRepository;
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Product>> GetProductsAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task AddProductAsync(Product item)
        {
            await _repository.AddAsync(item);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateProductAsync(Product item)
        {
            await _repository.UpdateAsync(item);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<bool> DoesProductIdExistsAsync(int id) => await _repository.ExistsAsync(x => x.Id == id);

        public async Task<bool> DoesProductNameExistsAsync(string name) => await _repository.ExistsAsync(x => x.Name == name);

        public async Task RemoveProductAsync(int id) 
        {
            if ((await _inventoryRepository.GetByProductIdAsync(id))?.Quantity > 0)
                throw new InvalidOperationException($"Cannot Delete Product ID {id} because it is still in stock");

            await _repository.DeleteAsync(id); 
            await _unitOfWork.SaveChangesAsync();
        }
    }
}

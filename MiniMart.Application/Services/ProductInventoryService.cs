using MiniMart.Application.Contracts;
using MiniMart.Application.Models;
using MiniMart.Domain.Models;

namespace MiniMart.Application.Services
{
    public class ProductInventoryService : IProductInventoryService
    {
        private readonly IProductInventoryRepository _productInvRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductRepository _productRepository;
        public ProductInventoryService(
            IProductInventoryRepository invRepository,
            IUnitOfWork unitOfWork,
            IProductRepository productRepository)
        {
            _productInvRepository = invRepository;
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceResponse> AddQuantityToInventoryAsync(int productId, int quantity)
        {
            if (!await _productRepository.ExistsAsync(x => x.Id == productId))
                return ServiceResponse.Failure(ServiceCodes.NotFound, "Product with id " + productId + " does not exist");

            var productInv = await _productInvRepository.GetByProductIdAsync(productId);
            if (productInv is null)
            {
                var newProductInv = new ProductInventory
                {
                    Quantity = quantity,
                    ProductId = productId,
                    DateCreated = DateTime.Now
                };

                await _productInvRepository.AddAsync(newProductInv);
                await _unitOfWork.SaveChangesAsync();
                return ServiceResponse.Success();
            }

            productInv.Quantity += quantity;
            productInv.DateUpdated = DateTime.Now;
            await _unitOfWork.SaveChangesAsync();
            return ServiceResponse.Success();
        }

        public async Task<ServiceResponse> RemoveQuantityFromInventory(int productId, int quantity)
        {
            if (!await _productRepository.ExistsAsync(x => x.Id == productId))
                return ServiceResponse.Failure(ServiceCodes.NotFound, "Product with id " + productId + " does not exist");

            var productInv = await _productInvRepository.GetByProductIdAsync(productId);
            if (productInv is null)
            {
                var newProductInv = new ProductInventory
                {
                    Quantity = 0,
                    ProductId = productId,
                    DateCreated = DateTime.Now
                };

                await _productInvRepository.AddAsync(newProductInv);
                await _unitOfWork.SaveChangesAsync();
                return ServiceResponse.Success();
            }

            if (productInv.Quantity -  quantity < 0) ServiceResponse.Failure(ServiceCodes.OperationError, "Insufficient inventory stock");
            productInv.Quantity -= quantity;
            productInv.DateUpdated = DateTime.Now;

            await _unitOfWork.SaveChangesAsync();
            return ServiceResponse.Success();
        }

        public async Task<IEnumerable<ProductInventory>> GetAvailableProductsInStockAsync() => await _productInvRepository.GetManyAsync(x => x.Quantity > 0);

        public async Task<IEnumerable<ProductInventory>> GetAllProductInventoriesAsync() => await _productInvRepository.GetAllAsync();

        public async Task<ProductInventory?> GetProductInventoryByProductIdAsync(int productId) =>
            await _productInvRepository.GetByProductIdAsync(productId);
    }
}

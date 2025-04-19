using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MiniMart.Application.Contracts;
using MiniMart.Domain.Models;

namespace MiniMart.Infrastructure.Repositories
{
    public class ProductInventoryRepository : RepositoryBase<ProductInventory>, IProductInventoryRepository
    {
        public ProductInventoryRepository(ApplicationDbContext context) : base(context) { }

        public async Task<ProductInventory?> GetByProductIdAsync(int productId) => 
            await _context.ProductInventories.Include(x => x.Product).FirstOrDefaultAsync(y => y.ProductId == productId);

        public async override Task<IEnumerable<ProductInventory>> GetAllAsync() =>
            await _context.ProductInventories.Include(y => y.Product).AsNoTracking().ToArrayAsync();

        public async override Task<IEnumerable<ProductInventory>> GetManyAsync(Expression<Func<ProductInventory, bool>> condition) =>
            await _context.ProductInventories.Include(y => y.Product).AsNoTracking().ToArrayAsync();
    }
}

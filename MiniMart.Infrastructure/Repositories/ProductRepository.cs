using MiniMart.Application.Contracts;
using MiniMart.Domain.Models;

namespace MiniMart.Infrastructure.Repositories
{
    public class ProductRepository : RepositoryBase<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context) { }
    }
}

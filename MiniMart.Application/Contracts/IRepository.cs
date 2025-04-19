using System.Linq.Expressions;
using Microsoft.Extensions.Hosting;

namespace MiniMart.Application.Contracts
{
    public interface IRepository<T>
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetManyAsync(Expression<Func<T, bool>> condition);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> condition);
        Task AddAsync(T item);
        Task UpdateAsync(T item);
        Task DeleteAsync(int id);
    }
}

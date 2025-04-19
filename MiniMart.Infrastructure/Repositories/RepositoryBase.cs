using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MiniMart.Application.Contracts;
using MiniMart.Domain.Models;

namespace MiniMart.Infrastructure.Repositories
{
    public abstract class RepositoryBase<T> : IRepository<T> where T : EntityBase
    {
        protected readonly ApplicationDbContext _context;

        public RepositoryBase(ApplicationDbContext context)
        {
            _context = context;
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _context.FindAsync<T>(id);
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> condition)
        {
            return await _context.Set<T>().AsNoTracking().FirstOrDefaultAsync(condition) != null;
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().AsNoTracking().ToArrayAsync();
        }

        public virtual async Task<IEnumerable<T>> GetManyAsync(Expression<Func<T, bool>> condition) =>
            await _context.Set<T>().Where(condition).AsNoTracking().ToArrayAsync();

        public virtual async Task AddAsync(T item)
        {
            await _context.AddAsync(item);
        }

        public virtual async Task UpdateAsync(T item)
        {
            _context.Update(item);
            await Task.CompletedTask;
        }

        public virtual async Task DeleteAsync(int id)
        {
            var item = await _context.FindAsync<T>(id);
            if (item != null)
                _context.Remove(item);
        }
    }
}

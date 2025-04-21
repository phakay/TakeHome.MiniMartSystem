using Microsoft.EntityFrameworkCore;
using MiniMart.Application.Contracts;
using MiniMart.Domain.Models;

namespace MiniMart.Infrastructure.Repositories
{
    public class TransactionQueryLogRepository : RepositoryBase<TransactionQueryLog>, ITransactionQueryLogRepository
    {
        public TransactionQueryLogRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<TransactionQueryLog?> GetByReferenceIdAsync(string refId)
        {
            return await _context.TransactionQueryLogs.FirstOrDefaultAsync(x => x.RefId == refId);
        }
    }
}

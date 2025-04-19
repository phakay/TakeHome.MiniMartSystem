using MiniMart.Application.Contracts;
using MiniMart.Domain.Models;

namespace MiniMart.Infrastructure.Repositories
{
    public class TransactionQueryLogRepository : RepositoryBase<TransactionQueryLog>, ITransactionQueryLogRepository
    {
        public TransactionQueryLogRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}

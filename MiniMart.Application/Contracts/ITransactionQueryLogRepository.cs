using MiniMart.Domain.Models;

namespace MiniMart.Application.Contracts
{
    public interface ITransactionQueryLogRepository : IRepository<TransactionQueryLog>
    {
        Task<TransactionQueryLog?> GetByReferenceIdAsync(string refId);
    }
}

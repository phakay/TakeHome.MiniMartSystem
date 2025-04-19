using MiniMart.Domain.Models;

namespace MiniMart.Application.Contracts
{
    public interface IStockAlertRepository : IRepository<StockAlert>
    {
        Task<IEnumerable<StockAlert>> GetStockAlertsAsync();
    }
}

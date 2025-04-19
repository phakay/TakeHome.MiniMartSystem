using MiniMart.Domain.Models;

namespace MiniMart.Application.Contracts
{
    public interface IStockAlertService
    {
        Task<IEnumerable<StockAlert>> GetStockAlertsAsync();
    }
}

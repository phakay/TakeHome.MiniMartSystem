using MiniMart.Application.Contracts;
using MiniMart.Domain.Models;

namespace MiniMart.Application.Services
{
    public class StockAlertService : IStockAlertService
    {
        private readonly IStockAlertRepository _repository;
        public StockAlertService(IStockAlertRepository repo)
        {
            _repository = repo;
        }

        public Task<IEnumerable<StockAlert>> GetStockAlertsAsync() => _repository.GetStockAlertsAsync();
    }
}

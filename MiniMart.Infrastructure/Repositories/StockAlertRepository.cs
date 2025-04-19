using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiniMart.Application.Contracts;
using MiniMart.Domain.Models;

namespace MiniMart.Infrastructure.Repositories
{
    public class StockAlertRepository : RepositoryBase<StockAlert>, IStockAlertRepository
    {
        public StockAlertRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<StockAlert>> GetStockAlertsAsync() =>
            await _context.StockAlerts.OrderByDescending(x => x.Date).AsNoTracking().ToArrayAsync();
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniMart.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace MiniMart.Infrastructure.Services
{
    public class InventoryStockLevelMonitorService : TimedService
    {
        private const int interval = 20 * 1000;
        private const int stockThreshold = 5;

        private ILogger<InventoryStockLevelMonitorService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public InventoryStockLevelMonitorService(
            IServiceScopeFactory svcScopeFactory,
            ILogger<InventoryStockLevelMonitorService> logger) : base(interval, logger)
        {
            _serviceScopeFactory = svcScopeFactory;
            _logger = logger;
        }

        protected override void RecurringJob()
        {
            _logger.LogInformation("Begin Monitoring Product Inventory Stock Level. last run: {lastRunDate}", _lastRun?.ToString("hh:mm:ss:ffff") ?? "00:00:00:0000");

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                if (ctx is null) throw new ApplicationException("Unable to reslove instance for type: " + typeof(ApplicationDbContext));

                var dt = DateTime.Now;
                var alerts = ctx.ProductInventories.Include(x => x.Product).Where(x => x.Quantity < stockThreshold).ToArray().Select(x =>
                {
                    return new StockAlert
                    {
                        Date = dt,
                        AlertMessage = $"Stock for {x.Product.Name} is low!. ProductId: {x.ProductId}. Current units: {x.Quantity}. Min units: {stockThreshold}"
                    };
                });

                if (alerts.Any())
                {
                    ctx.AddRange(alerts);
                    ctx.SaveChanges();
                }
            }
        }
    }
}

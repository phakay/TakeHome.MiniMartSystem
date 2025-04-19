using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniMart.Domain.Models;
using System.Text.Json;
using MiniMart.Application.Models;

namespace MiniMart.Infrastructure.Services
{
    public class StockReconcilationService : TimedService
    {
        private const int interval = 20 * 1000;

        private ILogger<StockReconcilationService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public StockReconcilationService(
          IServiceScopeFactory svcScopeFactory,
          ILogger<StockReconcilationService> logger) : base(interval, logger)
        {
            _serviceScopeFactory = svcScopeFactory;
            _logger = logger;
        }

        protected override void RecurringJob()
        {
            _logger.LogInformation("Begin Stock Reconcilation. last run: {lastRunDate}", _lastRun?.ToString("hh:mm:ss:ffff") ?? "00:00:00:0000");

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var failedOrders = ctx.PurchaseOrders.Where(x => x.OrderStatus == PurchaseStatus.Failed).ToArray();
                foreach (var order in failedOrders)
                {
                    try
                    {
                        var orderRequest = JsonSerializer.Deserialize<PurchaseRequest>(order.OrderData);
                        if (orderRequest is null)
                        {
                            _logger.LogError("Request data could not be resolved for order ID: {Id}", order.Id);
                            continue;
                        }

                        List<PurchaseItem> unProcessedLineItems = [];
                        foreach (var item in orderRequest.LineItems)
                        {
                            var productInv = ctx.ProductInventories.FirstOrDefault(x => x.ProductId == item.ProductId);
                            if (productInv is null)
                            {
                                _logger.LogError("Inventory record for ProductId: {ProductId} does not exist", item.ProductId);
                                unProcessedLineItems.Add(item);
                                continue;
                            }
                            productInv.Quantity += item.Quantity;
                        }

                        if (unProcessedLineItems.Count == 0)
                            order.OrderStatus = PurchaseStatus.Reconciled;
                        else
                        {
                            order.OrderData = JsonSerializer.Serialize(unProcessedLineItems);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An exception occurred while processing reconcilation for order ID: {Id}", order.Id);
                    }
                }

                ctx.SaveChanges();
            }
        }
    }
}

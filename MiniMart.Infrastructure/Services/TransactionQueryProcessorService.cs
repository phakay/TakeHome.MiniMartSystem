using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniMart.Domain.Models;
using MiniMart.Application.Contracts;
using MiniMart.Application.Models;

namespace MiniMart.Infrastructure.Services
{
    public class TransactionQueryProcessorService : TimedService
    {
        private const int interval = 20 * 1000;
        private const int requeryIntervalSinceLogDate = 30;
        private const int maxRetryCount = 5;

        private ILogger<TransactionQueryProcessorService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public TransactionQueryProcessorService(
            IServiceScopeFactory svcScopeFactory,
            ILogger<TransactionQueryProcessorService> logger) : base(interval, logger)
        {
            _serviceScopeFactory = svcScopeFactory;
            _logger = logger;
        }

        protected override void RecurringJob()
        {
            _logger.LogInformation("Begin Processing Transaction Query Records. last run: {lastRunDate}", _lastRun?.ToString("hh:mm:ss:ffff") ?? "00:00:00:0000");

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                if (ctx is null) throw new ApplicationException("Unable to reslove instance for type: " + typeof(ApplicationDbContext));

                var paymentSvc = scope.ServiceProvider.GetRequiredService<IExternalGatewayPaymentService>();
                if (paymentSvc is null) throw new ApplicationException("Unable to reslove instance for type: " + typeof(IExternalGatewayPaymentService));

                var pendingTsqs = ctx.TransactionQueryLogs.Where(x => x.Status == TransactionStatus.Pending &&
                     DateTime.Now > x.LogDate.AddSeconds(requeryIntervalSinceLogDate) && x.RetryCount < maxRetryCount).ToArray();

                var refs = pendingTsqs.Select(x => x.RefId).ToArray();

                var refsAndStatusToResolve = new Dictionary<string, bool>();

                foreach (var tsqRecord in pendingTsqs)
                {
                    try
                    {
                        tsqRecord.LastChecked = DateTime.Now;

                        // make gtw call to requery
                        var request = new QueryTransactionRequest
                        {
                            TransactionId = tsqRecord.RefId,
                        };

                        var response = paymentSvc.QueryTransactionStatusAsync(request).Result;

                        tsqRecord.RetryCount++;
                        if (response is null)
                        {
                            _logger.LogError("Response is null for refID: {RefId}", tsqRecord.RefId);
                            if (tsqRecord.RetryCount > maxRetryCount)
                            {
                                tsqRecord.StatusMessage = "Retry limit exceeded";
                            }
                            continue;
                        }

                        // handle result from gtw after requery
                        if (response.IsSuccessful)
                        {
                            tsqRecord.Status = TransactionStatus.Processed;
                            refsAndStatusToResolve.Add(tsqRecord.RefId, true);
                        }
                        else
                        { 
                            tsqRecord.Status = response.ShouldRequery ? TransactionStatus.Pending : TransactionStatus.Failed;
                            tsqRecord.StatusMessage = response.ErrorMessage;
                        }

                        if (tsqRecord.Status == TransactionStatus.Failed)
                        {
                            refsAndStatusToResolve.Add(tsqRecord.RefId, false);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An exception occurred while processing requery for tsq refID: {RefId}", tsqRecord.RefId);
                    }
                }

                if (refsAndStatusToResolve.Count > 0)
                {
                    UpdateOrdersStatus(ctx, refsAndStatusToResolve);
                }

                ctx.SaveChanges();
            }
        }

        private void UpdateOrdersStatus(ApplicationDbContext ctx, Dictionary<string, bool> refsSet)
        {
            var refs = refsSet.Keys;
            ctx.PurchaseOrders.Where(x => x.OrderStatus == PurchaseStatus.Pending && refs.Contains(x.TransactionReference)).ToList()
            .ForEach(order =>
            {
                order.OrderStatus = refsSet[order.TransactionReference] ? PurchaseStatus.Success : PurchaseStatus.Failed;
            });
        }
    }
}

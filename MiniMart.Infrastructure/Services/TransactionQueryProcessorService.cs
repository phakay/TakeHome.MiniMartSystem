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
                     DateTime.Now > x.LogDate.AddSeconds(requeryIntervalSinceLogDate)).ToArray();

                var refs = pendingTsqs.Select(x => x.RefId).ToArray();

                var refsAndStatusToResolve = new Dictionary<string, bool>();

                foreach (var tsqRecord in pendingTsqs)
                {
                    try
                    {
                        tsqRecord.LastChecked = DateTime.Now;

                        if (tsqRecord.RetryCount > maxRetryCount)
                        {
                            tsqRecord.StatusMessage = "retry limit exceeded";
                            tsqRecord.Status = TransactionStatus.Failed;
                            refsAndStatusToResolve.Add(tsqRecord.RefId, false);
                            continue;
                        }

                        // make gtw call to requery
                        var request = new QueryTransactionRequest
                        {
                            TransactionId = tsqRecord.RefId
                        };

                        var response = paymentSvc.QueryTransactionStatusAsync(request).Result;
                        if (response is null)
                        {
                            _logger.LogError("Response is null for refID: {RefId}", tsqRecord.RefId);
                            continue;
                        }

                        // handle result from gtw
                        if (response.isSuccessful)
                        {
                            tsqRecord.Status = TransactionStatus.Processed;
                            refsAndStatusToResolve.Add(tsqRecord.RefId, true);

                        }
                        else if (response.isFailed) 
                        { 
                            tsqRecord.Status = TransactionStatus.Failed;
                        }

                        tsqRecord.StatusMessage = response.message;
                        tsqRecord.RetryCount++;
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

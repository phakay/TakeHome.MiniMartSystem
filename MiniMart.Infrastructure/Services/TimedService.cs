using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MiniMart.Infrastructure.Services
{
    public abstract class TimedService : IHostedService, IAsyncDisposable
    {
        private Timer? _timer;
        private int _interval;
        private ILogger<TimedService> _logger;
        protected DateTime? _lastRun;

        public TimedService(int intervalInMilliseconds, ILogger<TimedService> logger)
        {
            _interval = intervalInMilliseconds;
            _logger = logger;
        }

        public async ValueTask DisposeAsync()
        {
            if (_timer == null)
                return;
            await _timer.DisposeAsync();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(HandleJob, null, _interval, Timeout.Infinite);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
            return Task.CompletedTask;
        }

        protected abstract void RecurringJob();

        private void HandleJob(object? _)
        {
            if (_timer is null) return;

            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            try
            {
                RecurringJob();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An exception occurred when running job");
            }
            finally
            {
                _lastRun = DateTime.Now;
                _timer.Change(_interval, Timeout.Infinite);
            }
        }
    }
}

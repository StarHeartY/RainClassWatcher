using RainClassWatcher.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RainClassWatcher.Services
{
    public sealed class RainClassPollingService
    {
        private readonly RainClassMonitor _monitor;
        private readonly TimeSpan _interval;

        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _pollingTask;

        public Action<RainClassScanResult>? ScanCompleted { get; set; }

        public Action<Exception>? ScanFailed { get; set; }

        public bool IsRunning => _cancellationTokenSource is not null;

        public RainClassPollingService(
            RainClassMonitor monitor,
            TimeSpan interval)
        {
            _monitor = monitor;
            _interval = interval;
        }

        public void Start()
        {
            if (IsRunning)
            {
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();

            _pollingTask = PollAsync(
                _cancellationTokenSource.Token);
        }

        public async Task StopAsync()
        {
            if (_cancellationTokenSource is null)
            {
                return;
            }

            _cancellationTokenSource.Cancel();

            try
            {
                if (_pollingTask is not null)
                {
                    await _pollingTask;
                }
            }
            catch (OperationCanceledException)
            {
            }

            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
            _pollingTask = null;
        }

        private async Task PollAsync(
            CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var result = await Task.Run(
                        _monitor.Scan,
                        cancellationToken);

                    ScanCompleted?.Invoke(result);
                }
                catch (OperationCanceledException)
                    when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    ScanFailed?.Invoke(ex);
                }

                await Task.Delay(
                    _interval,
                    cancellationToken);
            }
        }
    }
}
}
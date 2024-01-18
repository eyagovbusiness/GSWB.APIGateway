using APIGateway.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TGF.Common.ROP.HttpResult;

namespace APIGateway.Infrastructure.Services
{

    /// <summary>
    /// Provides functionality for cleaning up expired tokens as a background service.
    /// </summary>
    internal class TokenCleanupService : IHostedService, IDisposable
    {
        private readonly ILogger<TokenCleanupService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private Timer? _timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenCleanupService"/> class.
        /// </summary>
        /// <param name="aLogger">The logger instance.</param>
        /// <param name="aServiceProvider">The service provider to retrieve services.</param>
        public TokenCleanupService(ILogger<TokenCleanupService> aLogger, IServiceProvider aServiceProvider)
        {
            _logger = aLogger;
            _serviceProvider = aServiceProvider;
        }

        /// <summary>
        /// Starts the service.
        /// </summary>
        /// <param name="aCancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task StartAsync(CancellationToken aCancellationToken)
        {
            _logger.LogInformation("Token cleanup background service is starting.");

            var lCurrentTimeUtc = DateTime.UtcNow;
            var lNext3AmUtc = lCurrentTimeUtc.Date.AddHours(27); // This gets 3 AM for the next day.

            // Calculate the time span until the next 3 AM UTC occurrence
            var lDelay = lNext3AmUtc - lCurrentTimeUtc;

            _timer = new Timer(ExecuteExpiredRecordsCleanup, null, lDelay,
                TimeSpan.FromHours(24));

            return Task.CompletedTask;
        }

        /// <summary>
        /// Executes the cleanup of expired records.
        /// </summary>
        /// <param name="aState">State object passed to the callback method. This parameter is ignored in this implementation.</param>
        private async void ExecuteExpiredRecordsCleanup(object? aState)
        {
            try
            {
                _logger.LogInformation("Token cleanup background service cleanup tasks execution started.");

                using var lScope = _serviceProvider.CreateScope();
                var lTokenRepository = lScope.ServiceProvider.GetRequiredService<ITokenPairAuthRecordRepository>();

                await lTokenRepository.DeleteExpiredRecordsAsync()
                    .Tap(deletedCount => _logger.LogInformation("{RecordsDeleted} expired token records deleted.", deletedCount));

                _logger.LogInformation("Token cleanup background service cleanup tasks execution finished.");

            }
            catch (Exception lEx)
            {
                _logger.LogError(lEx, "An error occurred during token cleanup.");
            }

        }

        /// <summary>
        /// Stops the service.
        /// </summary>
        /// <param name="aCancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task StopAsync(CancellationToken aCancellationToken)
        {
            _logger.LogInformation("Token cleanup background service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        #region IDisposable

        private bool _disposed = false;

        /// <summary>
        /// Disposes managed and unmanaged resources.
        /// </summary>
        /// <param name="aDisposing">Indicates whether to dispose managed resources.</param>
        protected virtual void Dispose(bool aDisposing)
        {
            if (!_disposed)
            {
                if (aDisposing)
                {
                    _timer?.Dispose();
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Disposes all resources used by the current instance of the <see cref="TokenCleanupService"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

    }

}

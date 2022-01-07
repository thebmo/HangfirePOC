using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HangfirePOC
{
    public class CacheRefreshService : BackgroundService
    {
        private readonly IBackgroundJobClient _backgroundJobs;
        private readonly ILogger<CacheRefreshService> _logger;
        private readonly IMemoryCache _cache;

        private const string DEFAULT_Q = "default";
        private const string KEY = "test_key";

        public static string[] QUEUES = { DEFAULT_Q };

        public CacheRefreshService(
            IBackgroundJobClient backgroundJobs,
            IMemoryCache cache,
            ILogger<CacheRefreshService> logger)
        {
            _backgroundJobs = backgroundJobs ?? throw new ArgumentNullException(nameof(backgroundJobs));
            _cache = cache;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {

            _backgroundJobs.Enqueue(() => initalSetCache());

            RecurringJob.AddOrUpdate(
                    DEFAULT_Q,
                    () => CheckOrRefreshCache(),
                    Cron.Minutely);

            return Task.CompletedTask;
        }

        [Queue(DEFAULT_Q)]
        public void initalSetCache()
        {
            SetCache();
        }

        [Queue(DEFAULT_Q)]
        public void CheckOrRefreshCache()
        {
            if (!_cache.TryGetValue(KEY, out DateTime then))
            {
                _logger.LogError("CACHE NOT FOUND, REFRESHING!");
                SetCache();
            }
            else
            {
                _logger.LogCritical($"CACHE FOUND: {then}. REFRESHING!!!");
                SetCache();
            };
        }

        private void SetCache()
        {
            var now = DateTime.UtcNow;
            _logger.LogWarning($"setting cache time to: {now}");
            _cache.Set(KEY, now);
        }
    }
}

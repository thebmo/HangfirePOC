using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace HangfirePOC
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();

            services.AddHangfire(s => s
                .UseIgnoredAssemblyVersionTypeResolver()
                .UseInMemoryStorage());

            services.AddHangfireServer(opt => 
                opt.Queues = CacheRefreshService.QUEUES);

            services.AddHostedService<CacheRefreshService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

        }
    }
}

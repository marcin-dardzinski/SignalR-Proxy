using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SignalRProxy.IntegrationTests
{
    public class Startup<THub>
        where THub : Hub
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<THub>("/hub");
            });
        }
    }

    public class ApplicationFactory<THub> : WebApplicationFactory<Startup<THub>>
        where THub : Hub
    {
        private readonly Action<IServiceCollection>? configureServices;

        public ApplicationFactory(Action<IServiceCollection>? configureServices = null)
        {
            this.configureServices = configureServices;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddLogging(lb => lb.ClearProviders());
                configureServices?.Invoke(services);
            });
        }
        protected override IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup<THub>>();
                });
        }
    }
}
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;

namespace SignalRProxy.IntegrationTests
{

    public interface IHubContract
    {
        Task ServerMethod(int a, string b);
    }

    class SimpleHub : Hub, IHubContract
    {
        private readonly InvocationReporter reporter;

        public SimpleHub(InvocationReporter reporter)
        {
            this.reporter = reporter;
        }

        public Task ServerMethod(int a, string b)
        {
            reporter.Report(new object[] { a, b });
            return Task.CompletedTask;
        }
    }

    class InvocationReporter
    {
        public List<Invocation> Invocations { get; } = new List<Invocation>();

        public void Report(object[] args, [CallerMemberName] string name = "")
        {
            var invocation = new Invocation(name, args);
            Invocations.Add(invocation);
        }
    }

    readonly struct Invocation
    {
        public Invocation(string name, object[] args)
        {
            Name = name;
            Args = args;
        }

        public string Name { get; }
        public object[] Args { get; }
    }

    class ApplicationFactory<THub> : WebApplicationFactory<Startup<THub>>
        where THub : Hub
    {
        private readonly InvocationReporter reporter;

        public ApplicationFactory(InvocationReporter reporter)
        {
            this.reporter = reporter;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(reporter);
                services.AddLogging(lb => lb.ClearProviders());
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

    public class SendTests : IAsyncLifetime
    {
        private readonly InvocationReporter reporter;
        private readonly ApplicationFactory<SimpleHub> factory;
        private readonly HubConnection connection;

        public SendTests()
        {
            reporter = new InvocationReporter();
            factory = new ApplicationFactory<SimpleHub>(reporter);
            connection = new HubConnectionBuilder()
                .WithUrl(new Uri("http://localhost/hub"), opts => opts.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler())
                .Build();
        }

        [Fact]
        public async Task Send_works()
        {
            var client = SignalRProxyGenerator.CreateProxy<IHubContract>(connection);
            await connection.StartAsync();

            await client.ServerMethod(1, "s");
            await Task.Delay(500);

            var invocation = Assert.Single(reporter.Invocations);

            Assert.Equal(nameof(client.ServerMethod), invocation.Name);
            Assert.Equal(new object[] { 1, "s" }, invocation.Args);

            await connection.StopAsync();
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            await connection.DisposeAsync();
            factory.Dispose();
        }
    }
}
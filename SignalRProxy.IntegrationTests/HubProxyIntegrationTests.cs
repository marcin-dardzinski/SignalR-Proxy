using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace SignalRProxy.IntegrationTests
{

    public interface IHubContract
    {
        Task SendMethod(int a, string b);
        Task<int> InvokeMethod(int a, int b);
    }

    public interface IHubEvents
    {
        Task Event(int a, string b);
    }

    class SimpleHub : Hub<IHubEvents>, IHubContract
    {
        private readonly InvocationReporter reporter;

        public SimpleHub(InvocationReporter reporter)
        {
            this.reporter = reporter;
        }

        public Task SendMethod(int a, string b)
        {
            reporter.Report(new object[] { a, b });
            return Task.CompletedTask;
        }

        public Task<int> InvokeMethod(int a, int b)
        {
            return Task.FromResult(a + b);
        }
    }


    public class HubProxyIntegrationTests : IAsyncLifetime
    {
        private readonly InvocationReporter reporter;
        private readonly ApplicationFactory<SimpleHub> factory;
        private readonly HubConnection connection;
        private readonly IHubContext<SimpleHub, IHubEvents> hubContext;
        private readonly HubProxy<IHubContract> proxy;

        public HubProxyIntegrationTests()
        {
            reporter = new InvocationReporter();
            factory = new ApplicationFactory<SimpleHub>(services => services.AddSingleton(reporter));
            connection = new HubConnectionBuilder()
                .WithUrl(new Uri("http://localhost/hub"), opts => opts.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler())
                .Build();

            hubContext = factory.Services.GetRequiredService<IHubContext<SimpleHub, IHubEvents>>();
            proxy = SignalRProxyGenerator.CreateProxy<IHubContract>(connection);
        }

        [Fact]
        public async Task Send_works()
        {
            await proxy.Client.SendMethod(1, "s");
            await Task.Delay(500);

            var invocation = Assert.Single(reporter.Invocations);

            Assert.Equal(nameof(IHubContract.SendMethod), invocation.Name);
            Assert.Equal(new object[] { 1, "s" }, invocation.Args);
        }

        [Fact]
        public async Task Invoke_works()
        {
            var res = await proxy.Client.InvokeMethod(2, 2);

            Assert.Equal(4, res);
        }

        [Fact]
        public async Task Subscription_works()
        {
            var handler = Substitute.For<IHubEvents>();
            proxy.Subscribe(handler);

            await hubContext.Clients.All.Event(1, "s");

            await handler.Received()
                .Event(1, "s");
        }

        public async Task InitializeAsync()
        {
            await connection.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await connection.StopAsync();
            await connection.DisposeAsync();
            factory.Dispose();
        }
    }
}
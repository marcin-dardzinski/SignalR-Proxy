using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
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


    public class SendTests : IAsyncLifetime
    {
        private readonly InvocationReporter reporter;
        private readonly ApplicationFactory<SimpleHub> factory;
        private readonly HubConnection connection;

        public SendTests()
        {
            reporter = new InvocationReporter();
            factory = new ApplicationFactory<SimpleHub>(services => services.AddSingleton(reporter));
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
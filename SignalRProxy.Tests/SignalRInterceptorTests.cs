using System.Linq;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using NSubstitute;
using Xunit;

namespace SignalRProxy.Tests
{
    public class SignalRInterceptorTests
    {
        private static readonly ProxyGenerator generator = new ProxyGenerator();
        private readonly IHubConnection connection;
        private readonly SignalRInterceptor interceptor;
        private readonly IServerClient proxy;

        public SignalRInterceptorTests()
        {
            connection = Substitute.For<IHubConnection>();
            interceptor = new SignalRInterceptor(connection);
            proxy = generator.CreateInterfaceProxyWithoutTarget<IServerClient>(interceptor);
        }

        [Fact]
        public async Task Proxies_method_without_result_as_send()
        {
            await proxy.SendMethod(1, 2);
            await connection.Received()
                .SendAsync(nameof(IServerClient.SendMethod), Helpers.IsArrayArg<object>(1, 2));
        }

        [Fact]
        public async Task Proxies_methods_with_result_as_invoke()
        {
            connection.InvokeAsync(null!, null!, null!)
                .ReturnsForAnyArgs(1);

            var res = await proxy.InvokeMethod(1, 2);

            Assert.Equal(1, res);
            await connection.Received()
                .InvokeAsync(nameof(IServerClient.InvokeMethod), Helpers.IsArrayArg<object>(1, 2), typeof(int));
        }

        public interface IServerClient
        {
            Task SendMethod(int x, int y);
            Task<int> InvokeMethod(int x, int y);
            void InvalidA();
            int InvalidB();
        }
    }
}
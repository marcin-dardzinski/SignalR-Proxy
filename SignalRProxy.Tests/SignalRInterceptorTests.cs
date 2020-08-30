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

        public SignalRInterceptorTests()
        {
            connection = Substitute.For<IHubConnection>();
            interceptor = new SignalRInterceptor(connection);
        }

        [Fact]
        public async Task Proxies_simple_send_calls_to_underlying_connection()
        {
            var proxy = generator.CreateInterfaceProxyWithoutTarget<ISimpleServerClient>(interceptor);

            await proxy.ServerMethod(1, 2);

            await connection.Received().SendAsync(nameof(ISimpleServerClient.ServerMethod), Helpers.IsArrayArg<object>(1, 2));
        }
    }

    public interface ISimpleServerClient
    {
        Task ServerMethod(int x, int y);
    }
}
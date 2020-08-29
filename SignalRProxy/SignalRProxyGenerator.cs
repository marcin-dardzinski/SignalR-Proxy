using Castle.DynamicProxy;
using Microsoft.AspNetCore.SignalR.Client;

namespace SignalRProxy
{
    public static class SignalRProxyGenerator
    {
        private static readonly ProxyGenerator generator = new ProxyGenerator();

        public static TClient CreateProxy<TClient>(HubConnection connection)
            where TClient : class
        {
            return CreateProxy<TClient>(new HubConnectionAdapter(connection));
        }

        public static TClient CreateProxy<TClient>(IHubConnection connection)
            where TClient : class
        {
            var interceptor = new SignalRInterceptor(connection);
            return generator.CreateInterfaceProxyWithoutTarget<TClient>(interceptor);
        }
    }
}
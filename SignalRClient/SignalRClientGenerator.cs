using Castle.DynamicProxy;
using Microsoft.AspNetCore.SignalR.Client;

namespace SignalRClient
{
    public static class SignalRClientGenerator
    {
        private static readonly ProxyGenerator generator = new ProxyGenerator();

        public static TClient CreateClient<TClient>(HubConnection connection)
            where TClient : class
        {
            return CreateClient<TClient>(new HubConnectionAdapter(connection));
        }

        public static TClient CreateClient<TClient>(IHubConnection connection)
            where TClient : class
        {
            var interceptor = new SignalRInterceptor(connection);
            return generator.CreateInterfaceProxyWithoutTarget<TClient>(interceptor);
        }
    }
}
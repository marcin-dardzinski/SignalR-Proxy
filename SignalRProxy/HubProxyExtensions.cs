using System;

namespace SignalRProxy
{
    public static class HubProxyExtensions
    {
        public static IDisposable Subscribe<THandler>(this IHubProxy proxy, THandler handler)
            where THandler : class
        {
            return proxy.Subscribe(handler, typeof(THandler));
        }
    }
}
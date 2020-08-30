using System;
using Microsoft.AspNetCore.SignalR.Client;

namespace SignalRProxy
{
    public interface IHubProxy
    {
        IDisposable Subscribe(object handler, Type handlerType);
    }

    public class HubProxy<THubClient> : IHubProxy
        where THubClient : class
    {
        private readonly IHubConnection connection;
        private readonly IHandlerSubscriber subscriber;
        public HubConnection HubConnection => connection.TargetConnection;
        public THubClient Client { get; }

        public HubProxy(IHubConnection connection, THubClient client, IHandlerSubscriber subscriber)
        {
            this.connection = connection;
            this.subscriber = subscriber;
            Client = client;
        }

        public IDisposable Subscribe(object handler, Type handlerType)
        {
            return subscriber.Subscribe(handler, handlerType, connection);
        }
    }
}
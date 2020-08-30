using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace SignalRProxy
{
    public interface IHubConnection
    {
        HubConnection TargetConnection { get; }

        Task SendAsync(string methodName, object[] args, CancellationToken cancellationToken = default);
        Task<object> InvokeAsync(string methodName, object[] args, Type returnType, CancellationToken cancellation = default);

        IDisposable On(string methodName, Type[] args, Func<object[], object?, Task> handler, object? state);
    }

    public class HubConnectionAdapter : IHubConnection
    {
        public HubConnectionAdapter(HubConnection connection)
        {
            TargetConnection = connection;
        }

        public HubConnection TargetConnection { get; }

        public Task SendAsync(string methodName, object[] args, CancellationToken cancellationToken = default)
        {
            return TargetConnection.SendCoreAsync(methodName, args, cancellationToken);
        }

        public Task<object> InvokeAsync(string methodName, object[] args, Type returnType, CancellationToken cancellationToken = default)
        {
            return TargetConnection.InvokeCoreAsync(methodName, returnType, args, cancellationToken);
        }

        public IDisposable On(string methodName, Type[] args, Func<object[], object?, Task> handler, object? state)
        {
            return TargetConnection.On(methodName, args, handler, state);
        }
    }
}
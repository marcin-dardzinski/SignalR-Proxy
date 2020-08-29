using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace SignalRClient
{
    public interface IHubConnection
    {
        Task SendAsync(string methodName, object[] args, CancellationToken cancellationToken = default);
    }

    public class HubConnectionAdapter : IHubConnection
    {
        private readonly HubConnection connection;

        public HubConnectionAdapter(HubConnection connection)
        {
            this.connection = connection;
        }

        public Task SendAsync(string methodName, object[] args, CancellationToken cancellationToken = default)
        {
            return connection.SendCoreAsync(methodName, args, cancellationToken);
        }
    }
}
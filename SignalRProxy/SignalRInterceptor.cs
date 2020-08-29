using Castle.DynamicProxy;
using Microsoft.AspNetCore.SignalR.Client;

namespace SignalRProxy
{
    public class SignalRInterceptor : IInterceptor
    {
        private readonly IHubConnection connection;

        public SignalRInterceptor(IHubConnection connection)
        {
            this.connection = connection;
        }

        public void Intercept(IInvocation invocation)
        {
            invocation.ReturnValue = connection.SendAsync(invocation.Method.Name, invocation.Arguments);
        }
    }
}
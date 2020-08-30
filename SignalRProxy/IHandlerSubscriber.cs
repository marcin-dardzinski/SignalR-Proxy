using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SignalRProxy
{
    public interface IHandlerSubscriber
    {
        IDisposable Subscribe(object handler, Type handlerType, IHubConnection connection);
    }

    public class HandlerSubscriber : IHandlerSubscriber
    {
        public IDisposable Subscribe(object handler, Type handlerType, IHubConnection connection)
        {
            var subs = new List<IDisposable>();

            foreach (var method in handlerType.GetMethods())
            {
                var args = method.GetParameters().Select(p => p.ParameterType).ToArray();
                var state = new HandlerState(method, handler);

                var sub = connection.On(method.Name, args, Handler, state);
                subs.Add(sub);
            }

            return new BatchedDisposable(subs);
        }

        private static Task Handler(object[] args, object? state)
        {
            var handler = (HandlerState)state!;
            return (Task)handler.Method.Invoke(handler.Handler, args);
        }

        class BatchedDisposable : IDisposable
        {
            private readonly IEnumerable<IDisposable> disposables;

            public BatchedDisposable(IEnumerable<IDisposable> disposables)
            {
                this.disposables = disposables;
            }

            public void Dispose()
            {
                foreach (var d in disposables)
                {
                    d.Dispose();
                }
            }
        }
    }

    internal class HandlerState
    {
        public HandlerState(MethodInfo method, object handler)
        {
            Method = method;
            Handler = handler;
        }

        public MethodInfo Method { get; }
        public object Handler { get; }
    }

}
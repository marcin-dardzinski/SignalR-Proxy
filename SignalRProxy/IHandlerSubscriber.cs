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
                var sub = Subscribe(handler, method, connection);
                subs.Add(sub);
            }

            return new BatchedDisposable(subs);
        }

        private IDisposable Subscribe(object handler, MethodInfo method, IHubConnection connection)
        {
            var args = method.GetParameters().Select(p => p.ParameterType).ToArray();
            var state = new HandlerState(method, handler);
            var handlerFunc = HandlerForMethod(method);

            return connection.On(method.Name, args, handlerFunc, state);
        }

        private Func<object[], object?, Task> HandlerForMethod(MethodInfo method)
        {
            if (method.ReturnType == typeof(void))
            {
                return SyncHandler;
            }
            else if (method.ReturnType == typeof(Task))
            {
                return AsyncHandler;
            }
            else
            {
                throw new InvalidOperationException(
                    $"Unexpected return type parameter: `{method.ReturnType}` for event handler method: {method.Name}. It must be void or non generic Task");
            }
        }

        internal static Task SyncHandler(object[] args, object? state)
        {
            var handler = (HandlerState)state!;
            handler.Method.Invoke(handler.Handler, args);

            return Task.CompletedTask;
        }

        internal static Task AsyncHandler(object[] args, object? state)
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
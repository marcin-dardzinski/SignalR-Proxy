using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Castle.DynamicProxy;

namespace SignalRProxy
{
    public class SignalRInterceptor : IInterceptor
    {
        private static readonly MethodInfo TaskCastMethod = typeof(SignalRInterceptor).GetMethod(nameof(Cast), BindingFlags.Static | BindingFlags.NonPublic)!;
        private readonly ConcurrentDictionary<MethodInfo, Func<IInvocation, Task>> methodCache = new ConcurrentDictionary<MethodInfo, Func<IInvocation, Task>>();
        private readonly IHubConnection connection;

        public SignalRInterceptor(IHubConnection connection)
        {
            this.connection = connection;
        }

        public void Intercept(IInvocation invocation)
        {
            var method = methodCache.GetOrAdd(invocation.Method, CreateMethod);
            invocation.ReturnValue = method(invocation);
        }

        private Func<IInvocation, Task> CreateMethod(MethodInfo method)
        {
            var returnType = method.ReturnType;
            if (!typeof(Task).IsAssignableFrom(returnType))
            {
                throw new InvalidOperationException($"Hub invocation method `{method.Name}` must return a Task");
            }

            if (returnType == typeof(Task))
            {
                return inv => connection.SendAsync(method.Name, inv.Arguments);
            }
            else if (returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var actualReturnType = returnType.GetGenericArguments().Single();
                var castMethod = TaskCastMethod.MakeGenericMethod(new[] { actualReturnType });

                return inv =>
                {
                    var res = connection.InvokeAsync(method.Name, inv.Arguments, actualReturnType);
                    return (Task)castMethod.Invoke(null, new[] { res });
                };
            }

            throw new InvalidOperationException($"Task is assignable from {returnType}, but is not a Task, nor Task<>, should never happen");
        }

        private static async Task<TResult> Cast<TResult>(Task<object> task)
        {
            var res = await task;
            return res != null ? (TResult)res : default!;
        }
    }
}
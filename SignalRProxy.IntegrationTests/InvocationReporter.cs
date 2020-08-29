using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SignalRProxy.IntegrationTests
{
    class InvocationReporter
    {
        public List<Invocation> Invocations { get; } = new List<Invocation>();

        public void Report(object[] args, [CallerMemberName] string name = "")
        {
            var invocation = new Invocation(name, args);
            Invocations.Add(invocation);
        }
    }

    readonly struct Invocation
    {
        public Invocation(string name, object[] args)
        {
            Name = name;
            Args = args;
        }

        public string Name { get; }
        public object[] Args { get; }
    }
}
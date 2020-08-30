using System.Linq;
using NSubstitute;

namespace SignalRProxy.Tests
{
    public static class Helpers
    {
        public static ref T[] IsArrayArg<T>(params T[] expected)
        {
            return ref Arg.Is<T[]>(actual => actual.SequenceEqual(expected));
        }
    }
}
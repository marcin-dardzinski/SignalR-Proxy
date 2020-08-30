using System;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;

namespace SignalRProxy.Tests
{
    public class HandlerSubscriberTests
    {
        private readonly IHubConnection connection;
        private readonly HandlerSubscriber subscriber;

        public HandlerSubscriberTests()
        {
            connection = Substitute.For<IHubConnection>();
            subscriber = new HandlerSubscriber();
        }

        [Fact]
        public void Subscribes_to_all_methods_of_the_interface()
        {
            var handler = new ServerImpl();

            subscriber.Subscribe(handler, typeof(IServer), connection);

            connection.Received().On(
                nameof(handler.A),
                Helpers.IsArrayArg(typeof(int), typeof(string)),
                Arg.Any<Func<object[], object?, Task>>(),
                Arg.Is<HandlerState>(st => st.Handler == handler && st.Method.Name == nameof(handler.A)));

            connection.Received().On(
                nameof(handler.B),
                Helpers.IsArrayArg<Type>(),
                Arg.Any<Func<object[], object?, Task>>(),
                Arg.Is<HandlerState>(st => st.Handler == handler && st.Method.Name == nameof(handler.B)));
        }

        interface IServer
        {
            Task A(int x, string y);
            Task B();
        }

        class ServerImpl : IServer
        {
            public Task A(int x, string y)
            {
                throw new System.NotImplementedException();
            }

            public Task B()
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
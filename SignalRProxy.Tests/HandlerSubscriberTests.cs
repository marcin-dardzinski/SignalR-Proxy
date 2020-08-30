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
        public void Subscribes_to_all_methods_of_the_interface_both_sync_and_async()
        {
            var handler = Substitute.For<IHandler>();

            subscriber.Subscribe(handler, typeof(IHandler), connection);

            connection.Received().On(
                nameof(handler.A),
                Helpers.IsArrayArg(typeof(int), typeof(string)),
                HandlerSubscriber.AsyncHandler,
                Arg.Is<HandlerState>(st => st.Handler == handler && st.Method.Name == nameof(handler.A)));

            connection.Received().On(
                nameof(handler.B),
                Helpers.IsArrayArg(typeof(int)),
                HandlerSubscriber.SyncHandler,
                Arg.Is<HandlerState>(st => st.Handler == handler && st.Method.Name == nameof(handler.B)));
        }

        [Fact]
        public async Task Async_handler_correctly_calls_handler_method()
        {
            var method = typeof(IHandler).GetMethod("A")!;
            var handler = Substitute.For<IHandler>();

            var state = new HandlerState(method, handler);
            var args = new object[] { 1, "s" };

            await HandlerSubscriber.AsyncHandler(args, state);

            await handler.Received()
                .A(1, "s");
        }

        [Fact]
        public async Task Sync_handler_correctly_calls_handler_method_and_wraps_with_completed_task()
        {
            var method = typeof(IHandler).GetMethod("B")!;
            var handler = Substitute.For<IHandler>();

            var state = new HandlerState(method, handler);
            var args = new object[] { 1 };

            await HandlerSubscriber.SyncHandler(args, state);

            handler.Received()
                .B(1);
        }

        public interface IHandler
        {
            Task A(int x, string y);
            void B(int x);
        }
    }
}
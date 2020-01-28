﻿using System.Threading;
using System.Threading.Tasks;
using NATS.Client;
using NATS.Client.Internals;
using Xunit;

namespace UnitTests.Internals
{
    public class InFlightRequestTests
    {
        [Fact]
        public async Task Timeout_ThrowsNatsTimeoutException()
        {
            // Arrange
            var sut = new InFlightRequest("Foo", default, 1, _ => { });

            // Assert
            await Assert.ThrowsAsync<NATSTimeoutException>(() => sut.Waiter.Task);
        }

        [Fact]
        public async Task TimeoutWithToken_ThrowsTaskCanceledExcpetion()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            var sut = new InFlightRequest("Foo", cts.Token, 1, _ => { });

            // Assert
            await Assert.ThrowsAsync<NATSTimeoutException>(() => sut.Waiter.Task);
        }

        [Fact]
        public async Task Canceled_ThrowsTaskCanceledExcpetion()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            var sut = new InFlightRequest("Foo", cts.Token, 0, _ => { });

            // Act
            cts.Cancel();

            // Assert
            await Assert.ThrowsAsync<TaskCanceledException>(() => sut.Waiter.Task);
        }

        [Fact]
        public async Task CanceledWithTimeout_ThrowsNatsTimeoutException()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            var sut = new InFlightRequest("Foo", cts.Token, int.MaxValue, _ => { });

            // Act
            cts.Cancel();

            // Assert
            // NATSTimeoutException is somewhat unexpected here
            await Assert.ThrowsAsync<NATSTimeoutException>(() => sut.Waiter.Task);
        }

        [Fact]
        public void Dispose_InvokesOnCompletedDelegate()
        {
            // Arrange
            var onCompletedArg = "";
            var sut = new InFlightRequest("Foo", default, 0, id => { onCompletedArg = id; });

            // Act
            sut.Dispose();

            // Assert
            Assert.Equal("Foo", onCompletedArg);
        }

        [Fact]
        public void Dispose_DoesNotThrowForNullDelegate()
        {
            // Arrange
            var sut = new InFlightRequest("Foo", default, 0, null);

            // Act
            var ex = Record.Exception(() => sut.Dispose());

            // Assert
            Assert.Null(ex);
        }
    }
}

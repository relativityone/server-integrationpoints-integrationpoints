using System;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.Unit
{
    [TestFixture]
    public class CompositeCancellationTokenTests
    {
        [Test]
        public void CancellingBothTokensShouldNotThrow()
        {
            // Arrange
            CancellationTokenSource cancelCancellationTokenSource = new CancellationTokenSource();
            CancellationTokenSource drainStopCancellationTokenSource = new CancellationTokenSource();

            CompositeCancellationToken sut =
                new CompositeCancellationToken(cancelCancellationTokenSource.Token,
                    drainStopCancellationTokenSource.Token, new EmptyLogger());

            // Act & Assert
            Action action = () =>
            {
                cancelCancellationTokenSource.Cancel();
                drainStopCancellationTokenSource.Cancel();
            };

            action.Should().NotThrow();
            sut.AnyReasonCancellationToken.IsCancellationRequested.Should().BeTrue();
        }

        [Test]
        public void CancellingStopToken_ShouldCancelAnyReasonToken()
        {
            // Arrange
            CancellationTokenSource cancelCancellationTokenSource = new CancellationTokenSource();
            CancellationTokenSource drainStopCancellationTokenSource = new CancellationTokenSource();

            CompositeCancellationToken sut =
                new CompositeCancellationToken(cancelCancellationTokenSource.Token,
                    drainStopCancellationTokenSource.Token, new EmptyLogger());

            // Act
            cancelCancellationTokenSource.Cancel();

            // Assert
            sut.AnyReasonCancellationToken.IsCancellationRequested.Should().BeTrue();
        }

        [Test]
        public void CancellingDrainStopToken_ShouldCancelAnyReasonToken()
        {
            // Arrange
            CancellationTokenSource cancelCancellationTokenSource = new CancellationTokenSource();
            CancellationTokenSource drainStopCancellationTokenSource = new CancellationTokenSource();

            CompositeCancellationToken sut =
                new CompositeCancellationToken(cancelCancellationTokenSource.Token,
                    drainStopCancellationTokenSource.Token, new EmptyLogger());

            // Act
            drainStopCancellationTokenSource.Cancel();

            // Assert
            sut.AnyReasonCancellationToken.IsCancellationRequested.Should().BeTrue();
        }
    }
}

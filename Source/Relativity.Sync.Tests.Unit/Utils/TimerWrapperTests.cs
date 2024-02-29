using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Tests.Unit.Utils
{
    internal class TimerWrapperTests
    {
        [Test]
        public void Activate_ShouldActivateTimer()
        {
            // Arrange
            int executionCount = 0;

            TimerWrapper sut = new TimerWrapper(Mock.Of<IAPILog>());

            TimerCallback callback = (state) => ++executionCount;

            // Act
            sut.Activate(callback, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(500));

            Task.Delay(TimeSpan.FromSeconds(2)).GetAwaiter().GetResult();

            // Assert
            executionCount.Should().BePositive();
        }

        [Test]
        public void Activate_ShouldThrow_WhenCalledTwice()
        {
            // Arrange
            TimerWrapper sut = new TimerWrapper(Mock.Of<IAPILog>());

            Action activateTimer = () => sut.Activate((state) => { }, null, TimeSpan.FromHours(1), TimeSpan.FromHours(1));

            activateTimer();

            // Act & Assert
            activateTimer.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public async Task Dispose_ShouldStopTimer()
        {
            // Arrange
            int executionCount = 0;

            TimerWrapper sut = new TimerWrapper(Mock.Of<IAPILog>());

            TimerCallback callback = (state) => ++executionCount;

            sut.Activate(callback, null, TimeSpan.Zero, TimeSpan.FromMinutes(500));

            await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);

            int alreadyExecutionsCount = executionCount;

            // Act
            sut.Dispose();

            // Assert
            executionCount.Should().Be(alreadyExecutionsCount);
        }
    }
}

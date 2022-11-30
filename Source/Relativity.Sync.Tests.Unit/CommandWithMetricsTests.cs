using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Logging;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Tests.Unit
{
    [TestFixture]
    public class CommandWithMetricsTests
    {
        private Mock<ICommand<IConfiguration>> _innerCommandMock;
        private Mock<ISyncMetrics> _metricsMock;
        private Mock<IStopwatch> _stopwatchFake;

        private CommandWithMetrics<IConfiguration> _sut;

        [SetUp]
        public void SetUp()
        {
            _innerCommandMock = new Mock<ICommand<IConfiguration>>();
            _metricsMock = new Mock<ISyncMetrics>();
            _stopwatchFake = new Mock<IStopwatch>();

            _sut = new CommandWithMetrics<IConfiguration>(_innerCommandMock.Object, _metricsMock.Object, _stopwatchFake.Object);
        }

        [Test]
        public async Task CanExecuteAsync_ShouldCallCanExecuteInnerCommand()
        {
            // ACT
            await _sut.CanExecuteAsync(CancellationToken.None).ConfigureAwait(false);

            // ASSERT
            _innerCommandMock.Verify(x => x.CanExecuteAsync(CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_ShouldCallExecuteInnerCommand()
        {
            // ACT
            await _sut.ExecuteAsync(CompositeCancellationToken.None).ConfigureAwait(false);

            // ASSERT
            _innerCommandMock.Verify(x => x.ExecuteAsync(CompositeCancellationToken.None), Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_ShouldReportValidMetricName()
        {
            string expectedName = $"{nameof(IConfiguration)}.Execute";

            // ACT
            await _sut.ExecuteAsync(CompositeCancellationToken.None).ConfigureAwait(false);

            // ASSERT
            VerifySentMetric(m => m.Name == expectedName);
        }

        [Test]
        public async Task CanExecuteAsync_ShouldReportValidMetricName()
        {
            string expectedName = $"{nameof(IConfiguration)}.CanExecute";

            // ACT
            await _sut.CanExecuteAsync(CancellationToken.None).ConfigureAwait(false);

            // ASSERT
            VerifySentMetric(m => m.Name == expectedName);
        }

        [Test]
        public async Task ExecuteAsync_ShouldReportCompletedStatus()
        {
            // ACT
            await _sut.ExecuteAsync(CompositeCancellationToken.None).ConfigureAwait(false);

            // ASSERT
            VerifySentMetric(m => m.ExecutionStatus == ExecutionStatus.Completed);
        }

        [Test]
        public void ExecuteAsync_ShouldReportFailedStatus()
        {
            _innerCommandMock.Setup(x => x.ExecuteAsync(CompositeCancellationToken.None)).Throws<Exception>();

            // ACT
            Func<Task> action = () => _sut.ExecuteAsync(CompositeCancellationToken.None);

            // ASSERT
            action.Should().Throw<Exception>();

            VerifySentMetric(m => m.ExecutionStatus == ExecutionStatus.Failed);
        }

        [Test]
        public void ExecuteAsync_ShouldReportCanceledStatusWhenExecutionCanceledByThrowingException()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CompositeCancellationToken compositeCancellationToken = new CompositeCancellationToken(tokenSource.Token, CancellationToken.None, new EmptyLogger());
            _innerCommandMock.Setup(x => x.ExecuteAsync(compositeCancellationToken)).Throws<OperationCanceledException>();

            // ACT
            tokenSource.Cancel();
            Func<Task> action = () => _sut.ExecuteAsync(compositeCancellationToken);

            // ASSERT
            action.Should().Throw<OperationCanceledException>();

            VerifySentMetric(m => m.ExecutionStatus == ExecutionStatus.Canceled);
        }


        [Test]
        public async Task ExecuteAsync_ShouldReportCanceledStatusWhenExecutionCanceledGracefuly()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CompositeCancellationToken compositeCancellationToken = new CompositeCancellationToken(tokenSource.Token, CancellationToken.None, new EmptyLogger());

            // ACT
            tokenSource.Cancel();
            await _sut.ExecuteAsync(compositeCancellationToken).ConfigureAwait(false);

            // ASSERT
            VerifySentMetric(m => m.ExecutionStatus == ExecutionStatus.Canceled);
        }

        [Test]
        public async Task ExecuteAsync_ShouldMeasureExecuteTimeProperly()
        {
            const double expectedMilliseconds = 10;
            TimeSpan executionTime = TimeSpan.FromMilliseconds(expectedMilliseconds);
            _stopwatchFake.Setup(x => x.Elapsed).Returns(executionTime);

            // ACT
            await _sut.ExecuteAsync(CompositeCancellationToken.None).ConfigureAwait(false);

            // ASSERT
            VerifySentMetric(m =>
                m.Duration == expectedMilliseconds &&
                m.ExecutionStatus == ExecutionStatus.Completed);
        }

        [Test]
        public async Task CanExecuteAsync_ShouldMeasureCanExecuteTimeProperly()
        {
            const double expectedMilliseconds = 10;
            TimeSpan executionTime = TimeSpan.FromMilliseconds(expectedMilliseconds);
            _stopwatchFake.Setup(x => x.Elapsed).Returns(executionTime);

            // ACT
            await _sut.CanExecuteAsync(CancellationToken.None).ConfigureAwait(false);

            // ASSERT
            VerifySentMetric(m =>
                m.Duration == expectedMilliseconds &&
                m.ExecutionStatus == ExecutionStatus.Completed);
        }

        private void VerifySentMetric(Expression<Func<CommandMetric, bool>> validationFunc)
        {
            _metricsMock.Verify(x => x.Send(It.Is(validationFunc)));
        }
    }
}

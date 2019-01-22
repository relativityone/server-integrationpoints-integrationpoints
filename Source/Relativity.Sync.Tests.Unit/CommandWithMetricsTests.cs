using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public class CommandWithMetricsTests
	{
		private Mock<ICommand<IConfiguration>> _innerCommand;
		private Mock<ISyncMetrics> _metrics;
		private Mock<IStopwatch> _stopwatch;

		private CommandWithMetrics<IConfiguration> _command;

		[SetUp]
		public void SetUp()
		{
			_innerCommand = new Mock<ICommand<IConfiguration>>();
			_metrics = new Mock<ISyncMetrics>();
			_stopwatch = new Mock<IStopwatch>();

			_command = new CommandWithMetrics<IConfiguration>(_innerCommand.Object, _metrics.Object, _stopwatch.Object);
		}

		[Test]
		public async Task ItShouldCallCanExecuteInnerCommand()
		{
			// ACT
			await _command.CanExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			_innerCommand.Verify(x => x.CanExecuteAsync(CancellationToken.None), Times.Once);
		}

		[Test]
		public async Task ItShouldCallExecuteInnerCommand()
		{
			// ACT
			await _command.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			_innerCommand.Verify(x => x.ExecuteAsync(CancellationToken.None), Times.Once);
		}

		[Test]
		public async Task ItShouldReportValidMetricName()
		{
			const string expectedName = nameof(IConfiguration);

			// ACT
			await _command.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			_metrics.Verify(x => x.TimedOperation(expectedName, It.IsAny<TimeSpan>(), It.IsAny<CommandExecutionStatus>()), Times.Once);
		}

		[Test]
		public async Task ItShouldReportCompletedStatus()
		{
			// ACT
			await _command.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			_metrics.Verify(x => x.TimedOperation(It.IsAny<string>(), It.IsAny<TimeSpan>(), CommandExecutionStatus.Completed));
		}

		[Test]
		public void ItShouldReportFailedStatus()
		{
			_innerCommand.Setup(x => x.ExecuteAsync(CancellationToken.None)).Throws<Exception>();

			// ACT
			Func<Task> action = async () => await _command.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<Exception>();
			_metrics.Verify(x => x.TimedOperation(It.IsAny<string>(), It.IsAny<TimeSpan>(), CommandExecutionStatus.Failed));
		}

		[Test]
		public void ItShouldReportCanceledStatusWhenExecutionCanceledByThrowingException()
		{
			CancellationTokenSource tokenSource = new CancellationTokenSource();
			_innerCommand.Setup(x => x.ExecuteAsync(tokenSource.Token)).Throws<OperationCanceledException>();

			// ACT
			tokenSource.Cancel();
			Func<Task> action = async () => await _command.ExecuteAsync(tokenSource.Token).ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<OperationCanceledException>();
			_metrics.Verify(x => x.TimedOperation(It.IsAny<string>(), It.IsAny<TimeSpan>(), CommandExecutionStatus.Canceled));
		}


		[Test]
		public async Task ItShouldReportCanceledStatusWhenExecutionCanceledGracefuly()
		{
			CancellationTokenSource tokenSource = new CancellationTokenSource();

			// ACT
			tokenSource.Cancel();
			await _command.ExecuteAsync(tokenSource.Token).ConfigureAwait(false);

			// ASSERT
			_metrics.Verify(x => x.TimedOperation(It.IsAny<string>(), It.IsAny<TimeSpan>(), CommandExecutionStatus.Canceled));
		}

		[Test]
		public async Task ItShouldMeasureExecuteTimeProperly()
		{
			const double milliseconds = 10;
			TimeSpan executionTime = TimeSpan.FromMilliseconds(milliseconds);
			_stopwatch.Setup(x => x.Elapsed).Returns(executionTime);

			// ACT
			await _command.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			_metrics.Verify(x => x.TimedOperation(It.IsAny<string>(), It.Is<TimeSpan>(actualTimespan => 
				actualTimespan.Equals(executionTime)), CommandExecutionStatus.Completed));
		}

		[Test]
		public async Task ItShouldMeasureCanExecuteTimeProperly()
		{
			const double milliseconds = 10;
			TimeSpan executionTime = TimeSpan.FromMilliseconds(milliseconds);
			_stopwatch.Setup(x => x.Elapsed).Returns(executionTime);

			// ACT
			await _command.CanExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			_metrics.Verify(x => x.TimedOperation(It.IsAny<string>(), It.Is<TimeSpan>(actualTimespan =>
				actualTimespan.Equals(executionTime)), CommandExecutionStatus.Completed));
		}
	}
}
using System;
using System.Threading;
using System.Threading.Tasks;
using Banzai;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Logging;
using Relativity.Sync.Tests.Unit.Stubs;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	internal sealed class SyncNodeTests
	{
		private SyncNodeStub _instance;

		private SyncExecutionContext _executionContext;

		private Mock<ICommand<IConfiguration>> _command;
		private ProgressStub _progress;

		private const string _STEP_NAME = "step name";

		[SetUp]
		public void SetUp()
		{
			_progress = new ProgressStub();
			_executionContext = new SyncExecutionContext(_progress, CancellationToken.None);

			_command = new Mock<ICommand<IConfiguration>>();

			_instance = new SyncNodeStub(_command.Object, new EmptyLogger(), _STEP_NAME);
		}

		[Test]
		public async Task ItShouldExecuteCommand()
		{
			_command.Setup(x => x.CanExecuteAsync(CancellationToken.None)).ReturnsAsync(true);

			// ACT
			await _instance.ExecuteAsync(_executionContext).ConfigureAwait(false);

			// ASSERT
			_command.Verify(x => x.ExecuteAsync(CancellationToken.None), Times.Once);
		}

		[Test]
		public async Task ItShouldNotExecuteCommandWhenUnableTo()
		{
			_command.Setup(x => x.CanExecuteAsync(CancellationToken.None)).ReturnsAsync(false);

			// ACT
			await _instance.ExecuteAsync(_executionContext).ConfigureAwait(false);

			// ASSERT
			_command.Verify(x => x.ExecuteAsync(CancellationToken.None), Times.Never);
		}

		[Test]
		public async Task ItShouldReturnFailedResultOnException()
		{
			_command.Setup(x => x.CanExecuteAsync(CancellationToken.None)).ReturnsAsync(true);
			_command.Setup(x => x.ExecuteAsync(CancellationToken.None)).Throws<InvalidOperationException>();

			// ACT
			NodeResult result = await _instance.ExecuteAsync(_executionContext).ConfigureAwait(false);

			// ASSERT
			result.Status.Should().Be(NodeResultStatus.Failed);
			_executionContext.Results.Count.Should().Be(1);
			_executionContext.Results[0].Exception.Should().NotBeNull();
			_executionContext.Results[0].Exception.Should().BeOfType<InvalidOperationException>();
		}

		[Test]
		public async Task ItShouldReportProgress()
		{
			_command.Setup(x => x.CanExecuteAsync(CancellationToken.None)).ReturnsAsync(true);

			// ACT
			await _instance.ExecuteAsync(_executionContext).ConfigureAwait(false);

			// ASSERT
			_progress.SyncJobState.State.Should().Be(_STEP_NAME);
		}

		[TestCase(ExecutionStatus.Canceled, NodeResultStatus.Failed)]
		[TestCase(ExecutionStatus.Failed, NodeResultStatus.Failed)]
		[TestCase(ExecutionStatus.CompletedWithErrors, NodeResultStatus.SucceededWithErrors)]
		[TestCase(ExecutionStatus.Completed, NodeResultStatus.Succeeded)]
		public async Task ItShouldConvertExecutionResult(ExecutionStatus resultStatus, NodeResultStatus expectedStatus)
		{
			_command.Setup(x => x.CanExecuteAsync(CancellationToken.None)).ReturnsAsync(true);
			_command.Setup(x => x.ExecuteAsync(CancellationToken.None)).ReturnsAsync(new ExecutionResult(resultStatus, null, null));

			// ACT
			NodeResult result = await _instance.ExecuteAsync(_executionContext).ConfigureAwait(false);

			// ASSERT
			result.Status.Should().Be(expectedStatus);
		}
	}
}
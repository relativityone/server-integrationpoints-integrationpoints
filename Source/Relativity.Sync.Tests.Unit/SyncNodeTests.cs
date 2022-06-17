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
		private SyncJobProgressStub _syncJobProgress;

		private const string _STEP_NAME = "step name";

		[SetUp]
		public void SetUp()
		{
			_syncJobProgress = new SyncJobProgressStub();
			_executionContext = new SyncExecutionContext(_syncJobProgress, CompositeCancellationToken.None);

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
			_command.Verify(x => x.ExecuteAsync(CompositeCancellationToken.None), Times.Once);
		}

		[Test]
		public async Task ItShouldNotExecuteCommandWhenUnableTo()
		{
			_command.Setup(x => x.CanExecuteAsync(CancellationToken.None)).ReturnsAsync(false);

			// ACT
			await _instance.ExecuteAsync(_executionContext).ConfigureAwait(false);

			// ASSERT
			_command.Verify(x => x.ExecuteAsync(CompositeCancellationToken.None), Times.Never);
		}

		[Test]
		public async Task ItShouldReturnFailedResultOnException()
		{
			_command.Setup(x => x.CanExecuteAsync(CancellationToken.None)).ReturnsAsync(true);
			_command.Setup(x => x.ExecuteAsync(CompositeCancellationToken.None)).Throws<InvalidOperationException>();

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
			_syncJobProgress.SyncJobState.Id.Should().Be(_STEP_NAME);
		}

		[Test]
		public async Task ItShouldHaveStatusCompletedWhenCannotExecute()
		{
			_command.Setup(x => x.CanExecuteAsync(CancellationToken.None)).ReturnsAsync(false);

			// ACT
			await _instance.ExecuteAsync(_executionContext).ConfigureAwait(false);

			// ASSERT
			_syncJobProgress.SyncJobState.Status.Should().Be(SyncJobStatus.Completed);
		}

		[Test]
		public async Task ItShouldCancelPipeline()
		{
			CancellationTokenSource tokenSource = new CancellationTokenSource();
			CompositeCancellationToken compositeCancellationToken = new CompositeCancellationToken(tokenSource.Token, CancellationToken.None, new EmptyLogger());
			SyncExecutionContext context = new SyncExecutionContext(_syncJobProgress, compositeCancellationToken);
			tokenSource.Cancel();
			
			// ACT
			await _instance.ExecuteAsync(context).ConfigureAwait(false);

			// ASSERT
			_command.Verify(x => x.CanExecuteAsync(It.IsAny<CancellationToken>()), Times.Never);
			_command.Verify(x => x.ExecuteAsync(It.IsAny<CompositeCancellationToken>()), Times.Never);
		}

		[TestCase(ExecutionStatus.Canceled, NodeResultStatus.Succeeded)]
		[TestCase(ExecutionStatus.Failed, NodeResultStatus.Failed)]
		[TestCase(ExecutionStatus.CompletedWithErrors, NodeResultStatus.SucceededWithErrors)]
		[TestCase(ExecutionStatus.Completed, NodeResultStatus.Succeeded)]
		[TestCase(ExecutionStatus.Skipped, NodeResultStatus.NotRun)]
		public async Task ItShouldConvertExecutionResult(ExecutionStatus resultStatus, NodeResultStatus expectedStatus)
		{
			_command.Setup(x => x.CanExecuteAsync(CancellationToken.None)).ReturnsAsync(true);
			_command.Setup(x => x.ExecuteAsync(CompositeCancellationToken.None)).ReturnsAsync(new ExecutionResult(resultStatus, null, null));

			// ACT
			NodeResult result = await _instance.ExecuteAsync(_executionContext).ConfigureAwait(false);

			// ASSERT
			result.Status.Should().Be(expectedStatus);
		}

		[Test]
		public void ItShouldReportFailureWhenConstrainsFailedWithException()
		{
			_command.Setup(x => x.CanExecuteAsync(CancellationToken.None)).Throws<InvalidOperationException>();
			
			// ACT
			Func<Task> action = () => _instance.ExecuteAsync(_executionContext);

			// ASSERT
			action.Should().Throw<InvalidOperationException>();
			_syncJobProgress.SyncJobState.Id.Should().Be(_STEP_NAME);
			_syncJobProgress.SyncJobState.Exception.Should().BeOfType<InvalidOperationException>();
		}
	}
}
using System;
using System.Threading;
using System.Threading.Tasks;
using Banzai;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.SumReporting;
using Relativity.Sync.Nodes;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public sealed class SyncRootNodeTests
	{
		private SyncRootNode _sut;

		private SyncExecutionContext _syncExecutionContext;
		private CancellationToken _token;
		
		private Mock<ICommand<IJobStatusConsolidationConfiguration>> _jobStatusConsolidationCommandStub;
		private Mock<ICommand<INotificationConfiguration>> _notificationCommandFake;
		private Mock<ICommand<IJobCleanupConfiguration>> _jobCleanupCommandStub;
		private Mock<ICommand<IAutomatedWorkflowTriggerConfiguration>> _automatedWfTriggerCommand;
		private Mock<INode<SyncExecutionContext>> _childNodeStub;
		private Mock<ISyncLog> _loggerStub;
		
		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_token = CancellationToken.None;
		}

		[SetUp]
		public void SetUp()
		{
			_loggerStub = new Mock<ISyncLog>();
			var jobEndMetricsService = new Mock<IJobEndMetricsService>();
			_notificationCommandFake = new Mock<ICommand<INotificationConfiguration>>();
			_jobStatusConsolidationCommandStub = new Mock<ICommand<IJobStatusConsolidationConfiguration>>();
			_jobCleanupCommandStub = new Mock<ICommand<IJobCleanupConfiguration>>();
			_automatedWfTriggerCommand = new Mock<ICommand<IAutomatedWorkflowTriggerConfiguration>>();

			_childNodeStub = new Mock<INode<SyncExecutionContext>>();

			var progress = new Mock<IProgress<SyncJobState>>();
			_syncExecutionContext = new SyncExecutionContext(progress.Object, _token);

			_sut = new SyncRootNode(jobEndMetricsService.Object,
				_jobStatusConsolidationCommandStub.Object,
				_notificationCommandFake.Object,
				_jobCleanupCommandStub.Object,
				_automatedWfTriggerCommand.Object,
				_loggerStub.Object);
			_sut.AddChild(_childNodeStub.Object);
		}

		[Test]
		public async Task ItShouldSendNotificationsAfterExecution()
		{
			int index = 1;
			int nodeExecutionOrder = 0;
			int commandExecutionOrder = 0;
			_childNodeStub.Setup(x => x.ExecuteAsync(It.IsAny<IExecutionContext<SyncExecutionContext>>())).Callback(() => nodeExecutionOrder = index++);
			_notificationCommandFake.Setup(x => x.CanExecuteAsync(_token)).ReturnsAsync(true);
			_notificationCommandFake.Setup(x => x.ExecuteAsync(_token)).ReturnsAsync(ExecutionResult.Success()).Callback(() => commandExecutionOrder = index++);

			// ACT
			await _sut.ExecuteAsync(_syncExecutionContext).ConfigureAwait(false);

			// ASSERT
			nodeExecutionOrder.Should().Be(1);
			const int two = 2;
			commandExecutionOrder.Should().Be(two);
		}

		[Test]
		public async Task ItShouldSendNotificationsAfterExecutionFailed()
		{
			_notificationCommandFake.Setup(x => x.CanExecuteAsync(_token)).ReturnsAsync(true);
			_notificationCommandFake.Setup(x => x.ExecuteAsync(_token)).ReturnsAsync(ExecutionResult.Success());
			_childNodeStub.Setup(x => x.ExecuteAsync(It.IsAny<IExecutionContext<SyncExecutionContext>>())).Throws<InvalidOperationException>();

			// ACT
			NodeResult result = await _sut.ExecuteAsync(_syncExecutionContext).ConfigureAwait(false);

			// ASSERT
			result.Status.Should().Be(NodeResultStatus.Failed);
			_notificationCommandFake.Verify(x => x.ExecuteAsync(_token), Times.Once);
		}

		[Test]
		public async Task ItShouldNotSendNotificationsIfDisabled()
		{
			_notificationCommandFake.Setup(x => x.CanExecuteAsync(_token)).ReturnsAsync(false);

			// ACT
			await _sut.ExecuteAsync(_syncExecutionContext).ConfigureAwait(false);

			// ASSERT
			_notificationCommandFake.Verify(x => x.ExecuteAsync(_token), Times.Never);
		}
	}
}
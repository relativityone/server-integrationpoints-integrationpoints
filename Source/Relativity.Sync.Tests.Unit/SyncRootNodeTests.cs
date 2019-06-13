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
		private SyncRootNode _instance;

		private CancellationToken _token;

		private Mock<IJobEndMetricsService> _jobEndMetricsService;
		private Mock<ICommand<INotificationConfiguration>> _notificationCommand;
		private Mock<INode<SyncExecutionContext>> _childNode;
		private Mock<ISyncLog> _logger;

		private SyncExecutionContext _syncExecutionContext;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_token = CancellationToken.None;
		}

		[SetUp]
		public void SetUp()
		{
			_jobEndMetricsService = new Mock<IJobEndMetricsService>();
			_notificationCommand = new Mock<ICommand<INotificationConfiguration>>();
			_logger = new Mock<ISyncLog>();

			_childNode = new Mock<INode<SyncExecutionContext>>();

			var progress = new Mock<IProgress<SyncJobState>>();
			_syncExecutionContext = new SyncExecutionContext(progress.Object, _token);

			_instance = new SyncRootNode(_jobEndMetricsService.Object, _notificationCommand.Object, _logger.Object);
			_instance.AddChild(_childNode.Object);
		}

		[Test]
		public async Task ItShouldSendNotificationsAfterExecution()
		{
			int index = 1;
			int nodeExecutionOrder = 0;
			int commandExecutionOrder = 0;
			_childNode.Setup(x => x.ExecuteAsync(It.IsAny<IExecutionContext<SyncExecutionContext>>())).Callback(() => nodeExecutionOrder = index++);
			_notificationCommand.Setup(x => x.CanExecuteAsync(_token)).ReturnsAsync(true);
			_notificationCommand.Setup(x => x.ExecuteAsync(_token)).ReturnsAsync(ExecutionResult.Success()).Callback(() => commandExecutionOrder = index++);

			// ACT
			await _instance.ExecuteAsync(_syncExecutionContext).ConfigureAwait(false);

			// ASSERT
			nodeExecutionOrder.Should().Be(1);
			const int two = 2;
			commandExecutionOrder.Should().Be(two);
		}

		[Test]
		public async Task ItShouldSendNotificationsAfterExecutionFailed()
		{
			_notificationCommand.Setup(x => x.CanExecuteAsync(_token)).ReturnsAsync(true);
			_notificationCommand.Setup(x => x.ExecuteAsync(_token)).ReturnsAsync(ExecutionResult.Success());
			_childNode.Setup(x => x.ExecuteAsync(It.IsAny<IExecutionContext<SyncExecutionContext>>())).Throws<InvalidOperationException>();

			// ACT
			NodeResult result = await _instance.ExecuteAsync(_syncExecutionContext).ConfigureAwait(false);

			// ASSERT
			result.Status.Should().Be(NodeResultStatus.Failed);
			_notificationCommand.Verify(x => x.ExecuteAsync(_token), Times.Once);
		}

		[Test]
		public void ItShouldNotThrowIfNotificationCommandFailed()
		{
			const string expectedExceptionMessage = "FooBarBaz";
			_notificationCommand.Setup(x => x.CanExecuteAsync(_token)).ReturnsAsync(true);
			_notificationCommand.Setup(x => x.ExecuteAsync(_token)).ReturnsAsync(ExecutionResult.Failure(new InvalidOperationException(expectedExceptionMessage)));

			// ACT
			Func<Task<NodeResult>> action = async () => await _instance.ExecuteAsync(_syncExecutionContext).ConfigureAwait(false);

			// ASSERT
			action.Should().NotThrow();
			_logger.Verify(x => x.LogError(
				It.Is<Exception>(ex => ex.Message.Equals(expectedExceptionMessage, StringComparison.InvariantCulture)),
				It.IsAny<string>(),
				It.IsAny<object[]>()));
		}

		[Test]
		public async Task ItShouldNotSendNotificationsIfDisabled()
		{
			_notificationCommand.Setup(x => x.CanExecuteAsync(_token)).ReturnsAsync(false);

			// ACT
			await _instance.ExecuteAsync(_syncExecutionContext).ConfigureAwait(false);

			// ASSERT
			_notificationCommand.Verify(x => x.ExecuteAsync(_token), Times.Never);
		}
	}
}
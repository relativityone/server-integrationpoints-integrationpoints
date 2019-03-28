﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Banzai;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Nodes;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public sealed class SyncRootNodeTests
	{
		private SyncRootNode _instance;

		private Mock<ICommand<INotificationConfiguration>> _command;
		private Mock<INode<SyncExecutionContext>> _childNode;
		private SyncExecutionContext _syncExecutionContext;
		private Mock<ISyncLog> _logger;

		[SetUp]
		public void SetUp()
		{
			_command = new Mock<ICommand<INotificationConfiguration>>();

			_childNode = new Mock<INode<SyncExecutionContext>>();

			_logger = new Mock<ISyncLog>();

			IProgress<SyncProgress> progress = new EmptyProgress<SyncProgress>();
			_syncExecutionContext = new SyncExecutionContext(progress, CancellationToken.None);

			_instance = new SyncRootNode(_command.Object, _logger.Object);
			_instance.AddChild(_childNode.Object);
		}

		[Test]
		public async Task ItShouldSendNotificationsAfterExecution()
		{
			int index = 1;
			int nodeExecutionOrder = 0;
			int commandExecutionOrder = 0;
			_childNode.Setup(x => x.ExecuteAsync(It.IsAny<IExecutionContext<SyncExecutionContext>>())).Callback(() => nodeExecutionOrder = index++);
			_command.Setup(x => x.CanExecuteAsync(CancellationToken.None)).ReturnsAsync(true);
			_command.Setup(x => x.ExecuteAsync(CancellationToken.None)).Returns(Task.FromResult(ExecutionResult.Success())).Callback(() => commandExecutionOrder = index++);

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
			_command.Setup(x => x.CanExecuteAsync(CancellationToken.None)).ReturnsAsync(true);
			_command.Setup(x => x.ExecuteAsync(CancellationToken.None)).ReturnsAsync(ExecutionResult.Success());
			_childNode.Setup(x => x.ExecuteAsync(It.IsAny<IExecutionContext<SyncExecutionContext>>())).Throws<InvalidOperationException>();

			// ACT
			NodeResult result = await _instance.ExecuteAsync(_syncExecutionContext).ConfigureAwait(false);

			// ASSERT
			result.Status.Should().Be(NodeResultStatus.Failed);
			_command.Verify(x => x.ExecuteAsync(CancellationToken.None), Times.Once);
		}

		[Test]
		public void ItShouldNotThrowIfNotificationCommandFailed()
		{
			const string expectedExceptionMessage = "FooBarBaz";
			_command.Setup(x => x.CanExecuteAsync(CancellationToken.None)).ReturnsAsync(true);
			_command.Setup(x => x.ExecuteAsync(CancellationToken.None)).ReturnsAsync(ExecutionResult.Failure(new Exception(expectedExceptionMessage)));

			// ACT
			Func<Task<NodeResult>> action = async () => await _instance.ExecuteAsync(_syncExecutionContext).ConfigureAwait(false);
			//NodeResult result = await _instance.ExecuteAsync(_syncExecutionContext).ConfigureAwait(false);

			// ASSERT
			action.Should().NotThrow();//.WithMessage("*FooBarBaz*");
			_logger.Verify(x => x.LogError(
				It.Is<Exception>(ex => ex.Message.Equals(expectedExceptionMessage, StringComparison.InvariantCulture)),
				It.IsAny<string>(),
				It.IsAny<object[]>()));
		}

		[Test]
		public async Task ItShouldNotSendNotificationsIfDisabled()
		{
			_command.Setup(x => x.CanExecuteAsync(CancellationToken.None)).ReturnsAsync(false);

			// ACT
			await _instance.ExecuteAsync(_syncExecutionContext).ConfigureAwait(false);

			// ASSERT
			_command.Verify(x => x.ExecuteAsync(CancellationToken.None), Times.Never);
		}
	}
}
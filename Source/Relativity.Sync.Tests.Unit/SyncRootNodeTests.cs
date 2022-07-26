using System;
using System.Threading;
using System.Threading.Tasks;
using Banzai;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.SumReporting;
using Relativity.Sync.Logging;
using Relativity.Sync.Nodes;

namespace Relativity.Sync.Tests.Unit
{
    [TestFixture]
    public sealed class SyncRootNodeTests
    {
        private SyncRootNode _sut;

        private SyncExecutionContext _syncExecutionContext;
        private CancellationTokenSource _drainStopTokenSource;
        private CompositeCancellationToken _token;
        
        private Mock<ICommand<IJobStatusConsolidationConfiguration>> _jobStatusConsolidationCommandFake;
        private Mock<ICommand<INotificationConfiguration>> _notificationCommandFake;
        private Mock<ICommand<IJobCleanupConfiguration>> _jobCleanupCommandFake;
        private Mock<ICommand<IAutomatedWorkflowTriggerConfiguration>> _automatedWorkflowTriggerCommandFake;

        private Mock<IJobEndMetricsService> _jobEndMetricsService;
        private Mock<IJobEndMetricsServiceFactory> _jobEndMetricsServiceFactory;
        private Mock<INode<SyncExecutionContext>> _childNodeFake;
        private Mock<IAPILog> _loggerFake;
        
        [SetUp]
        public void SetUp()
        {
            _drainStopTokenSource = new CancellationTokenSource();
            _token = new CompositeCancellationToken(CancellationToken.None, _drainStopTokenSource.Token, new EmptyLogger());

            var progress = new Mock<IProgress<SyncJobState>>();
            _syncExecutionContext = new SyncExecutionContext(progress.Object, _token);

            _jobStatusConsolidationCommandFake = new Mock<ICommand<IJobStatusConsolidationConfiguration>>();
            
            _notificationCommandFake = new Mock<ICommand<INotificationConfiguration>>();

            _jobCleanupCommandFake = new Mock<ICommand<IJobCleanupConfiguration>>();
            _automatedWorkflowTriggerCommandFake = new Mock<ICommand<IAutomatedWorkflowTriggerConfiguration>>();

            _jobEndMetricsService = new Mock<IJobEndMetricsService>();
            _jobEndMetricsServiceFactory = new Mock<IJobEndMetricsServiceFactory>();
            _jobEndMetricsServiceFactory.Setup(x => x.CreateJobEndMetricsService(It.IsAny<bool>())).Returns(_jobEndMetricsService.Object);

            _childNodeFake = new Mock<INode<SyncExecutionContext>>();

            _loggerFake = new Mock<IAPILog>();

            _sut = new SyncRootNode(_jobEndMetricsServiceFactory.Object,
                _jobStatusConsolidationCommandFake.Object,
                _notificationCommandFake.Object,
                _jobCleanupCommandFake.Object,
                _automatedWorkflowTriggerCommandFake.Object,
                _loggerFake.Object);
            _sut.AddChild(_childNodeFake.Object);
        }

        [Test]
        public async Task ExecuteAsync_ShouldSendNotificationsAfterExecution()
        {
            // ARRANGE
            int index = 1;
            int nodeExecutionOrder = 0;
            int commandExecutionOrder = 0;
            _childNodeFake.Setup(x => x.ExecuteAsync(It.IsAny<IExecutionContext<SyncExecutionContext>>())).Callback(() => nodeExecutionOrder = index++);

            _notificationCommandFake.Setup(x => x.CanExecuteAsync(_token.StopCancellationToken)).ReturnsAsync(true);
            _notificationCommandFake.Setup(x => x.ExecuteAsync(_token)).ReturnsAsync(ExecutionResult.Success()).Callback(() => commandExecutionOrder = index++);

            // ACT
            await _sut.ExecuteAsync(_syncExecutionContext).ConfigureAwait(false);

            // ASSERT
            nodeExecutionOrder.Should().Be(1);
            const int two = 2;
            commandExecutionOrder.Should().Be(two);
        }

        [Test]
        public async Task ExecuteAsync_ShouldSendNotifications_WhenExecutionFailed()
        {
            // ARRANGE
            _notificationCommandFake.Setup(x => x.CanExecuteAsync(_token.StopCancellationToken)).ReturnsAsync(true);
            _notificationCommandFake.Setup(x => x.ExecuteAsync(_token)).ReturnsAsync(ExecutionResult.Success());
            _childNodeFake.Setup(x => x.ExecuteAsync(It.IsAny<IExecutionContext<SyncExecutionContext>>())).Throws<InvalidOperationException>();

            // ACT
            NodeResult result = await _sut.ExecuteAsync(_syncExecutionContext).ConfigureAwait(false);

            // ASSERT
            result.Status.Should().Be(NodeResultStatus.Failed);
            _notificationCommandFake.Verify(x => x.ExecuteAsync(_token), Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_ShouldNotSendNotifications_WhenNotificationIsDisabled()
        {
            // ARRANGE
            _notificationCommandFake.Setup(x => x.CanExecuteAsync(_token.StopCancellationToken)).ReturnsAsync(false);

            // ACT
            await _sut.ExecuteAsync(_syncExecutionContext).ConfigureAwait(false);

            // ASSERT
            _notificationCommandFake.Verify(x => x.ExecuteAsync(_token), Times.Never);
        }

        [Test]
        public async Task ExecuteAsync_ShouldNotExecuteAfterExecuteOperations_WhenJobIsSuspended()
        {
            // ARRANGE
            _drainStopTokenSource.Cancel();

            // ACT
            await _sut.ExecuteAsync(_syncExecutionContext).ConfigureAwait(false);

            // ASSERT
            _automatedWorkflowTriggerCommandFake.Verify(x => x.ExecuteAsync(It.IsAny<CompositeCancellationToken>()), Times.Never);
            _jobCleanupCommandFake.Verify(x => x.ExecuteAsync(It.IsAny<CompositeCancellationToken>()), Times.Never);
            _jobStatusConsolidationCommandFake.Verify(x => x.ExecuteAsync(It.IsAny<CompositeCancellationToken>()), Times.Never);
            _notificationCommandFake.Verify(x => x.ExecuteAsync(It.IsAny<CompositeCancellationToken>()), Times.Never);
        }
    }
}

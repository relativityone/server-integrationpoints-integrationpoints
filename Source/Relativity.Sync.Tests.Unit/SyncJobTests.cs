using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Banzai;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.Logging;
using Relativity.Sync.Tests.Unit.Stubs;
using Relativity.Sync.Toggles;
using Relativity.Sync.Toggles.Service;

namespace Relativity.Sync.Tests.Unit
{
    [TestFixture]
    public sealed class SyncJobTests
    {
        private SyncJob _instance;

        private NodeWithResultStub _pipeline;
        private ISyncExecutionContextFactory _executionContextFactory;
        private SyncJobParameters _syncJobParameters;
        private ExecutionOptions _executionOptions;
        private Mock<ISyncToggles> _syncToggles;
        private Mock<IJobProgressUpdaterFactory> _jobProgressUpdaterFactoryMock;
        private Mock<IJobProgressUpdater> _jobProgressUpdater;

        private readonly Guid _WORKFLOW_ID = Guid.NewGuid();

        [SetUp]
        public void SetUp()
        {
            _pipeline = new NodeWithResultStub();

            _executionContextFactory = new SyncExecutionContextFactory(new SyncJobExecutionConfiguration());

            _executionOptions = new ExecutionOptions
            {
                ThrowOnError = true
            };

            _syncToggles = new Mock<ISyncToggles>();
            _jobProgressUpdaterFactoryMock = new Mock<IJobProgressUpdaterFactory>();
            _jobProgressUpdater = new Mock<IJobProgressUpdater>();
            _jobProgressUpdaterFactoryMock.Setup(x => x.CreateJobProgressUpdater()).Returns(_jobProgressUpdater.Object);

            _syncJobParameters = new SyncJobParameters(0, 0, 0, _WORKFLOW_ID, Guid.Empty);
            _instance = PrepareSut(_pipeline);
        }

        [TestCase(NodeResultStatus.Succeeded)]
        [TestCase(NodeResultStatus.SucceededWithErrors)]
        public async Task ItShouldExecuteJob(NodeResultStatus nonErrorStatus)
        {
            _pipeline.ResultStatus = nonErrorStatus;

            // ACT
            await _instance.ExecuteAsync(CompositeCancellationToken.None).ConfigureAwait(false);

            // ASSERT
            Assert.Pass();
        }

        [Test]
        public void ItShouldThrowExceptionWhenJobFailed()
        {
            _pipeline.ResultStatus = NodeResultStatus.Failed;

            // ACT
            Func<Task> action = () => _instance.ExecuteAsync(CompositeCancellationToken.None);

            // ASSERT
            action.Should().Throw<SyncException>().Which.WorkflowId.Should().Be(_WORKFLOW_ID.ToString());
        }

        [Test]
        public void ItShouldPassOperationCanceledException()
        {
            FailingNodeStub<OperationCanceledException> pipeline = new FailingNodeStub<OperationCanceledException>(_executionOptions);
            SyncJob instance = PrepareSut(pipeline);

            // ACT
            Func<Task> action = () => instance.ExecuteAsync(CompositeCancellationToken.None);

            // ASSERT
            action.Should().Throw<OperationCanceledException>();
        }

        [Test]
        public void ItShouldPassSyncException()
        {
            FailingNodeStub<SyncException> pipeline = new FailingNodeStub<SyncException>(_executionOptions);
            SyncJob instance = PrepareSut(pipeline);

            // ACT
            Func<Task> action = () => instance.ExecuteAsync(CompositeCancellationToken.None);

            // ASSERT
            action.Should().Throw<SyncException>();
        }

        [Test]
        public void ItShouldChangeExceptionToSyncException()
        {
            FailingNodeStub<IOException> pipeline = new FailingNodeStub<IOException>(_executionOptions);
            SyncJob instance = PrepareSut(pipeline);

            // ACT
            Func<Task> action = () => instance.ExecuteAsync(CompositeCancellationToken.None);

            // ASSERT
            action.Should().Throw<SyncException>().Which.WorkflowId.Should().Be(_WORKFLOW_ID.ToString());
        }

        [Test]
        public async Task ItShouldInvokeSyncProgress()
        {
            INode<SyncExecutionContext> pipeline = new NodeWithProgressStub();
            var syncProgressMock = new Mock<IProgress<SyncJobState>>();
            _instance = new SyncJob(pipeline, _executionContextFactory, _syncJobParameters, syncProgressMock.Object, _syncToggles.Object, _jobProgressUpdaterFactoryMock.Object, new EmptyLogger());

            // ACT
            await _instance.ExecuteAsync(CompositeCancellationToken.None).ConfigureAwait(false);

            // ASSERT
            syncProgressMock.Verify(x => x.Report(It.IsAny<SyncJobState>()));
        }

        [Test]
        public async Task ItShouldInvokeBothProgresses()
        {
            INode<SyncExecutionContext> pipeline = new NodeWithProgressStub();
            var syncProgressMock = new Mock<IProgress<SyncJobState>>();
            var customProgressMock = new Mock<IProgress<SyncJobState>>();
            _instance = new SyncJob(pipeline, _executionContextFactory, _syncJobParameters, syncProgressMock.Object, _syncToggles.Object, _jobProgressUpdaterFactoryMock.Object, new EmptyLogger());

            // ACT
            await _instance.ExecuteAsync(customProgressMock.Object, CompositeCancellationToken.None).ConfigureAwait(false);

            // ASSERT
            syncProgressMock.Verify(x => x.Report(It.IsAny<SyncJobState>()));
            customProgressMock.Verify(x => x.Report(It.IsAny<SyncJobState>()));
        }

        [Test]
        public async Task ItShouldNotThrowWhenCustomProgressThrows()
        {
            INode<SyncExecutionContext> pipeline = new NodeWithProgressStub();
            var progressMock = new Mock<IProgress<SyncJobState>>();
            progressMock.Setup(x => x.Report(It.IsAny<SyncJobState>())).Throws<InvalidOperationException>();
            _instance = PrepareSut(pipeline);

            // ACT
            await _instance.ExecuteAsync(progressMock.Object, CompositeCancellationToken.None).ConfigureAwait(false);

            // ASSERT
            progressMock.Verify(x => x.Report(It.IsAny<SyncJobState>()));
        }

        [Test]
        public async Task ExecuteAsync_ShouldUpdateJobHistoryStatus_WhenSuccess()
        {
            // Arrange
            _syncToggles.Setup(x => x.IsEnabled<EnableJobHistoryStatusUpdateToggle>()).Returns(true);
            SyncJob job = PrepareSut(new NodeWithProgressStub());

            // Act
            await job.ExecuteAsync(CompositeCancellationToken.None);

            // Assert
            _jobProgressUpdater.Verify(x => x.SetJobStartedAsync(), Times.Once);
            _jobProgressUpdater.Verify(x => x.UpdateJobStatusAsync(JobHistoryStatus.Completed), Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_ShouldUpdateJobHistoryStatus_WhenStopped()
        {
            // Arrange
            _syncToggles.Setup(x => x.IsEnabled<EnableJobHistoryStatusUpdateToggle>()).Returns(true);
            CancellationTokenSource stopToken = new CancellationTokenSource();
            CancellationTokenSource drainStopToken = new CancellationTokenSource();
            CompositeCancellationToken compositeToken = new CompositeCancellationToken(stopToken.Token, drainStopToken.Token, new EmptyLogger());

            SyncJob job = PrepareSut(new NodeWithProgressStub());

            // Act
            stopToken.Cancel();
            await job.ExecuteAsync(compositeToken);

            // Assert
            _jobProgressUpdater.Verify(x => x.SetJobStartedAsync(), Times.Once);
            _jobProgressUpdater.Verify(x => x.UpdateJobStatusAsync(JobHistoryStatus.Stopped), Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_ShouldUpdateJobHistoryStatus_WhenStoppedWithException()
        {
            // Arrange
            _syncToggles.Setup(x => x.IsEnabled<EnableJobHistoryStatusUpdateToggle>()).Returns(true);

            Mock<INode<SyncExecutionContext>> pipeline = new Mock<INode<SyncExecutionContext>>();
            pipeline
                .Setup(x => x.ExecuteAsync(It.IsAny<IExecutionContext<SyncExecutionContext>>()))
                .Throws<OperationCanceledException>();

            SyncJob job = PrepareSut(pipeline.Object);

            // Act
            await job.ExecuteAsync(CompositeCancellationToken.None);

            // Assert
            _jobProgressUpdater.Verify(x => x.SetJobStartedAsync(), Times.Once);
            _jobProgressUpdater.Verify(x => x.UpdateJobStatusAsync(JobHistoryStatus.Stopped), Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_ShouldUpdateJobHistoryStatus_WhenDrainStopped()
        {
            // Arrange
            _syncToggles.Setup(x => x.IsEnabled<EnableJobHistoryStatusUpdateToggle>()).Returns(true);
            CancellationTokenSource stopToken = new CancellationTokenSource();
            CancellationTokenSource drainStopToken = new CancellationTokenSource();
            CompositeCancellationToken compositeToken = new CompositeCancellationToken(stopToken.Token, drainStopToken.Token, new EmptyLogger());

            SyncJob job = PrepareSut(new NodeWithProgressStub());

            // Act
            drainStopToken.Cancel();
            await job.ExecuteAsync(compositeToken);

            // Assert
            _jobProgressUpdater.Verify(x => x.SetJobStartedAsync(), Times.Once);
            _jobProgressUpdater.Verify(x => x.UpdateJobStatusAsync(JobHistoryStatus.Suspended), Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_ShouldUpdateJobHistoryStatus_WhenValidationFailed()
        {
            // Arrange
            _syncToggles.Setup(x => x.IsEnabled<EnableJobHistoryStatusUpdateToggle>()).Returns(true);

            SyncJob job = PrepareSut(new ValidationFailedNode());

            // Act
            await job.ExecuteAsync(CompositeCancellationToken.None);

            // Assert
            _jobProgressUpdater.Verify(x => x.SetJobStartedAsync(), Times.Once);
            _jobProgressUpdater.Verify(x => x.UpdateJobStatusAsync(JobHistoryStatus.ValidationFailed), Times.Once);
            _jobProgressUpdater.Verify(x => x.AddJobErrorAsync(It.IsAny<ValidationException>()), Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_ShouldUpdateJobHistoryStatus_WhenJobFailed()
        {
            // Arrange
            _syncToggles.Setup(x => x.IsEnabled<EnableJobHistoryStatusUpdateToggle>()).Returns(true);

            Mock<INode<SyncExecutionContext>> pipeline = new Mock<INode<SyncExecutionContext>>();
            pipeline
                .Setup(x => x.ExecuteAsync(It.IsAny<IExecutionContext<SyncExecutionContext>>()))
                .Throws<Exception>();

            SyncJob job = PrepareSut(pipeline.Object);

            // Act
            await job.ExecuteAsync(CompositeCancellationToken.None);

            // Assert
            _jobProgressUpdater.Verify(x => x.SetJobStartedAsync(), Times.Once);
            _jobProgressUpdater.Verify(x => x.UpdateJobStatusAsync(JobHistoryStatus.Failed), Times.Once);
            _jobProgressUpdater.Verify(x => x.AddJobErrorAsync(It.IsAny<Exception>()), Times.Once);
        }

        private SyncJob PrepareSut(INode<SyncExecutionContext> pipeline)
        {
            return new SyncJob(pipeline, _executionContextFactory, _syncJobParameters, new EmptyProgress<SyncJobState>(), _syncToggles.Object, _jobProgressUpdaterFactoryMock.Object, new EmptyLogger());
        }

        private class ValidationFailedNode : Node<SyncExecutionContext>
        {
            protected override Task<NodeResultStatus> PerformExecuteAsync(IExecutionContext<SyncExecutionContext> context)
            {
                context.Subject.Results.Add(new ExecutionResult(ExecutionStatus.Failed, "Validation failed", new ValidationException()));
                return Task.FromResult(NodeResultStatus.Failed);
            }
        }
    }
}

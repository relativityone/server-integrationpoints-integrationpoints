using System;
using System.IO;
using System.Threading.Tasks;
using Banzai;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Logging;
using Relativity.Sync.Tests.Unit.Stubs;

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

            _syncJobParameters = new SyncJobParameters(0, 0, 0, _WORKFLOW_ID);
            _instance = new SyncJob(_pipeline, _executionContextFactory, _syncJobParameters, new EmptyProgress<SyncJobState>(), new EmptyLogger());
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
            SyncJob instance = new SyncJob(pipeline, _executionContextFactory, _syncJobParameters, new EmptyProgress<SyncJobState>(), new EmptyLogger());

            // ACT
            Func<Task> action = () => instance.ExecuteAsync(CompositeCancellationToken.None);

            // ASSERT
            action.Should().Throw<OperationCanceledException>();
        }

        [Test]
        public void ItShouldPassSyncException()
        {
            FailingNodeStub<SyncException> pipeline = new FailingNodeStub<SyncException>(_executionOptions);
            SyncJob instance = new SyncJob(pipeline, _executionContextFactory, _syncJobParameters, new EmptyProgress<SyncJobState>(), new EmptyLogger());

            // ACT
            Func<Task> action = () => instance.ExecuteAsync(CompositeCancellationToken.None);

            // ASSERT
            action.Should().Throw<SyncException>();
        }

        [Test]
        public void ItShouldChangeExceptionToSyncException()
        {
            FailingNodeStub<IOException> pipeline = new FailingNodeStub<IOException>(_executionOptions);
            SyncJob instance = new SyncJob(pipeline, _executionContextFactory, _syncJobParameters, new EmptyProgress<SyncJobState>(), new EmptyLogger());

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
            _instance = new SyncJob(pipeline, _executionContextFactory, _syncJobParameters, syncProgressMock.Object, new EmptyLogger());

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
            _instance = new SyncJob(pipeline, _executionContextFactory, _syncJobParameters, syncProgressMock.Object, new EmptyLogger());

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
            _instance = new SyncJob(pipeline, _executionContextFactory, _syncJobParameters, new EmptyProgress<SyncJobState>(), new EmptyLogger());

            // ACT
            await _instance.ExecuteAsync(progressMock.Object, CompositeCancellationToken.None).ConfigureAwait(false);

            // ASSERT
            progressMock.Verify(x => x.Report(It.IsAny<SyncJobState>()));
        }
    }
}
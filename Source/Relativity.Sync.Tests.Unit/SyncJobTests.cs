using System;
using System.IO;
using System.Threading;
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
		private CorrelationId _correlationId;
		private ExecutionOptions _executionOptions;

		private const string _CORRELATION_ID = "id";

		[SetUp]
		public void SetUp()
		{
			_pipeline = new NodeWithResultStub();

			_executionContextFactory = new SyncExecutionContextFactory(new SyncJobExecutionConfiguration());

			_executionOptions = new ExecutionOptions
			{
				ThrowOnError = true
			};

			_correlationId = new CorrelationId(_CORRELATION_ID);
			_instance = new SyncJob(_pipeline, _executionContextFactory, _correlationId, new EmptyProgress<SyncJobState>(), new EmptyLogger());
		}

		[TestCase(NodeResultStatus.Succeeded)]
		[TestCase(NodeResultStatus.SucceededWithErrors)]
		public async Task ItShouldExecuteJob(NodeResultStatus nonErrorStatus)
		{
			_pipeline.ResultStatus = nonErrorStatus;

			// ACT
			await _instance.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			Assert.Pass();
		}

		[Test]
		public void ItShouldThrowExceptionWhenJobFailed()
		{
			_pipeline.ResultStatus = NodeResultStatus.Failed;

			// ACT
			Func<Task> action = async () => await _instance.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<SyncException>().Which.CorrelationId.Should().Be(_CORRELATION_ID);
		}

		[Test]
		public void ItShouldPassOperationCanceledException()
		{
			FailingNodeStub<OperationCanceledException> pipeline = new FailingNodeStub<OperationCanceledException>(_executionOptions);
			SyncJob instance = new SyncJob(pipeline, _executionContextFactory, _correlationId, new EmptyProgress<SyncJobState>(), new EmptyLogger());

			// ACT
			Func<Task> action = async () => await instance.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<OperationCanceledException>();
		}

		[Test]
		public void ItShouldPassSyncException()
		{
			FailingNodeStub<SyncException> pipeline = new FailingNodeStub<SyncException>(_executionOptions);
			SyncJob instance = new SyncJob(pipeline, _executionContextFactory, _correlationId, new EmptyProgress<SyncJobState>(), new EmptyLogger());

			// ACT
			Func<Task> action = async () => await instance.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<SyncException>();
		}

		[Test]
		public void ItShouldChangeExceptionToSyncException()
		{
			FailingNodeStub<IOException> pipeline = new FailingNodeStub<IOException>(_executionOptions);
			SyncJob instance = new SyncJob(pipeline, _executionContextFactory, _correlationId, new EmptyProgress<SyncJobState>(), new EmptyLogger());

			// ACT
			Func<Task> action = async () => await instance.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<SyncException>().Which.CorrelationId.Should().Be(_CORRELATION_ID);
		}

		[Test]
		public void ItShouldRetryJob()
		{
			Func<Task> actionWithoutProgress = async () => await _instance.RetryAsync(CancellationToken.None).ConfigureAwait(false);
			Func<Task> actionWithProgress = async () => await _instance.RetryAsync(Mock.Of<IProgress<SyncJobState>>(), CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			actionWithoutProgress.Should().Throw<NotImplementedException>();
			actionWithProgress.Should().Throw<NotImplementedException>();
		}

		[Test]
		public async Task ItShouldInvokeSyncProgress()
		{
			INode<SyncExecutionContext> pipeline = new NodeWithProgressStub();
			var syncProgressMock = new Mock<IProgress<SyncJobState>>();
			_instance = new SyncJob(pipeline, _executionContextFactory, _correlationId, syncProgressMock.Object, new EmptyLogger());

			// ACT
			await _instance.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			syncProgressMock.Verify(x => x.Report(It.IsAny<SyncJobState>()));
		}

		[Test]
		public async Task ItShouldInvokeBothProgresses()
		{
			INode<SyncExecutionContext> pipeline = new NodeWithProgressStub();
			var syncProgressMock = new Mock<IProgress<SyncJobState>>();
			var customProgressMock = new Mock<IProgress<SyncJobState>>();
			_instance = new SyncJob(pipeline, _executionContextFactory, _correlationId, syncProgressMock.Object, new EmptyLogger());

			// ACT
			await _instance.ExecuteAsync(customProgressMock.Object, CancellationToken.None).ConfigureAwait(false);

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
			_instance = new SyncJob(pipeline, _executionContextFactory, _correlationId, new EmptyProgress<SyncJobState>(), new EmptyLogger());

			// ACT
			await _instance.ExecuteAsync(progressMock.Object, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			progressMock.Verify(x => x.Report(It.IsAny<SyncJobState>()));
		}
	}
}
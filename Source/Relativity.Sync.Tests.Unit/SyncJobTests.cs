using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Banzai;
using FluentAssertions;
using NUnit.Framework;
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

			_executionContextFactory = new SyncExecutionContextFactory(new SyncConfiguration());

			_executionOptions = new ExecutionOptions
			{
				ThrowOnError = true
			};

			_correlationId = new CorrelationId(_CORRELATION_ID);
			_instance = new SyncJob(_pipeline, _executionContextFactory, _correlationId, new EmptyLogger());
		}

		[Test]
		public async Task ItShouldExecuteJob()
		{
			_pipeline.ResultStatus = NodeResultStatus.Succeeded;

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
			SyncJob instance = new SyncJob(pipeline, _executionContextFactory, _correlationId, new EmptyLogger());

			// ACT
			Func<Task> action = async () => await instance.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<OperationCanceledException>();
		}

		[Test]
		public void ItShouldPassSyncException()
		{
			FailingNodeStub<SyncException> pipeline = new FailingNodeStub<SyncException>(_executionOptions);
			SyncJob instance = new SyncJob(pipeline, _executionContextFactory, _correlationId, new EmptyLogger());

			// ACT
			Func<Task> action = async () => await instance.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<SyncException>();
		}

		[Test]
		public void ItShouldChangeExceptionToSyncException()
		{
			FailingNodeStub<IOException> pipeline = new FailingNodeStub<IOException>(_executionOptions);
			SyncJob instance = new SyncJob(pipeline, _executionContextFactory, _correlationId, new EmptyLogger());

			// ACT
			Func<Task> action = async () => await instance.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<SyncException>().Which.CorrelationId.Should().Be(_CORRELATION_ID);
		}

		[Test]
		public void ItShouldRetryJob()
		{
			Func<Task> action = async () => await _instance.RetryAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<NotImplementedException>();
		}
	}
}
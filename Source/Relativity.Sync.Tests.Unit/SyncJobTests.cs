using System;
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

		[SetUp]
		public void SetUp()
		{
			_pipeline = new NodeWithResultStub();

			ISyncExecutionContextFactory contextFactory = new SyncExecutionContextFactory(new SyncConfiguration());

			_instance = new SyncJob(_pipeline, contextFactory);
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
			action.Should().Throw<SyncException>();
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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Tests.Integration.Stubs;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	public sealed class SyncLogWriterTests
	{
		private ISyncJob _syncJob;
		private Mock<ISyncLog> _logger;

		[SetUp]
		public void SetUp()
		{
			_logger = new Mock<ISyncLog>();

			SyncJobFactory factory = new SyncJobFactory();
			_syncJob = factory.Create(IntegrationTestsContainerBuilder.CreateContainer(), new List<IInstaller>(), new SyncJobParameters(1, 1), new SyncConfiguration(), _logger.Object);
		}

		[Test]
		public async Task BanzaiShouldWriteToSyncLog()
		{
			await _syncJob.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			// this is one of the invocations happening in Banzai
			_logger.Verify(x => x.LogDebug(null, "Added child node."));
		}
	}
}
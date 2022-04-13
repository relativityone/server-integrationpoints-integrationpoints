using Relativity.API;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Banzai.Logging;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Logging;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.Integration.Helpers;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	public sealed class SyncLogWriterTests
	{
		private ISyncJob _syncJob;
		private Mock<IAPILog> _logger;

		[SetUp]
		public void SetUp()
		{
			ContainerBuilder containerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
			IntegrationTestsContainerBuilder.MockAllSteps(containerBuilder);
			IntegrationTestsContainerBuilder.MockReportingWithProgress(containerBuilder);

			_logger = new Mock<IAPILog>();
			LogWriter.SetFactory(new SyncLogWriterFactory(_logger.Object));

			_syncJob = containerBuilder.Build().Resolve<ISyncJob>();
		}

		[Test]
		public async Task BanzaiShouldWriteToSyncLog()
		{
			await _syncJob.ExecuteAsync(CompositeCancellationToken.None).ConfigureAwait(false);

			// ASSERT
			// this is one of the invocations happening in Banzai
			_logger.Verify(x => x.LogDebug(null, "Added child node."));
		}
	}
}

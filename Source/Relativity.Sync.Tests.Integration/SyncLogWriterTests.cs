﻿using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Banzai.Logging;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Logging;
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
			ContainerBuilder containerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
			IntegrationTestsContainerBuilder.MockAllSteps(containerBuilder);
			IntegrationTestsContainerBuilder.MockMetrics(containerBuilder);

			_logger = new Mock<ISyncLog>();
			LogWriter.SetFactory(new SyncLogWriterFactory(_logger.Object));

			_syncJob = containerBuilder.Build().Resolve<ISyncJob>();
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
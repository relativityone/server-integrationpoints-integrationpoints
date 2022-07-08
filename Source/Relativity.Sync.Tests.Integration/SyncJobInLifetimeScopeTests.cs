using System;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Sync.Logging;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.Integration.Helpers;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	public class SyncJobInLifetimeScopeTests
	{
		private Mock<IContainerFactory> _containerFactory;
		private IAPILog _logger;
		private SyncJobParameters _syncJobParameters;
		private IRelativityServices _relativityServices;
		private SyncJobExecutionConfiguration _configuration;
		private Mock<ISyncJob> _syncJob;

		[SetUp]
		public void SetUp()
		{
			_syncJob = new Mock<ISyncJob>();
			
			_containerFactory = new Mock<IContainerFactory>();
            _containerFactory
                .Setup(x => x.RegisterSyncDependencies(It.IsAny<ContainerBuilder>(), It.IsAny<SyncJobParameters>(), It.IsAny<IRelativityServices>(), It.IsAny<SyncJobExecutionConfiguration>(), It.IsAny<IAPILog>()))
                .Callback((ContainerBuilder builder, SyncJobParameters syncJobParameters, IRelativityServices relativityServices, SyncJobExecutionConfiguration config, IAPILog logger) =>
                {
                    builder.RegisterInstance(_syncJob.Object).As<ISyncJob>();
				});
			_syncJobParameters = FakeHelper.CreateSyncJobParameters();
			_relativityServices = ContainerHelper.CreateMockedRelativityServices();
			_configuration = new SyncJobExecutionConfiguration();
			_logger = new EmptyLogger();
		}

		[Test]
		public async Task ItShouldPassExecuteAsync()
		{
			var instance = new SyncJobInLifetimeScope(_containerFactory.Object, _syncJobParameters, _relativityServices, _configuration, _logger);

			await instance.ExecuteAsync(CompositeCancellationToken.None).ConfigureAwait(false);

			_syncJob.Verify(x => x.ExecuteAsync(CompositeCancellationToken.None), Times.Once);
		}

		[Test]
		public async Task ItShouldPassExecuteWithProgressAsync()
		{
			IProgress<SyncJobState> progress = Mock.Of<IProgress<SyncJobState>>();
			var instance = new SyncJobInLifetimeScope(_containerFactory.Object, _syncJobParameters, _relativityServices, _configuration, _logger);

			await instance.ExecuteAsync(progress, CompositeCancellationToken.None).ConfigureAwait(false);

			_syncJob.Verify(x => x.ExecuteAsync(progress, CompositeCancellationToken.None), Times.Once);
		}
		
		[Test]
		public async Task ItShouldThrowSyncException_WhenCannotResolveSyncJobFromContainer()
		{
			var instance = new SyncJobInLifetimeScope(Mock.Of<IContainerFactory>(), _syncJobParameters, _relativityServices, _configuration, _logger);

			Func<Task> func = () => instance.ExecuteAsync(CompositeCancellationToken.None);

			await func.Should().ThrowAsync<SyncException>().ConfigureAwait(false);
		}
	}
}
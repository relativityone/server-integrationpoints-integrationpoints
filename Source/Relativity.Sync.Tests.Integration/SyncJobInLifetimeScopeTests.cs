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
		private IContainer _container;
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

			var containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterInstance(_syncJob.Object).As<ISyncJob>();
			_container = containerBuilder.Build();

			_containerFactory = new Mock<IContainerFactory>();
			_syncJobParameters = FakeHelper.CreateSyncJobParameters();
			_relativityServices = ContainerHelper.CreateMockedRelativityServices();
			_configuration = new SyncJobExecutionConfiguration();
			_logger = new EmptyLogger();
		}

		[Test]
		public async Task ItShouldPassExecuteAsync()
		{
			var instance = new SyncJobInLifetimeScope(_containerFactory.Object, _container, _syncJobParameters, _relativityServices, _configuration, _logger);

			await instance.ExecuteAsync(CompositeCancellationToken.None).ConfigureAwait(false);

			_syncJob.Verify(x => x.ExecuteAsync(CompositeCancellationToken.None), Times.Once);
		}

		[Test]
		public async Task ItShouldPassExecuteWithProgressAsync()
		{
			IProgress<SyncJobState> progress = Mock.Of<IProgress<SyncJobState>>();
			var instance = new SyncJobInLifetimeScope(_containerFactory.Object, _container, _syncJobParameters, _relativityServices, _configuration, _logger);

			await instance.ExecuteAsync(progress, CompositeCancellationToken.None).ConfigureAwait(false);

			_syncJob.Verify(x => x.ExecuteAsync(progress, CompositeCancellationToken.None), Times.Once);
		}
		
		[Test]
		public async Task ItShouldThrowSyncException()
		{
			var containerBuilder = new ContainerBuilder();
			IContainer containerWithoutSyncJob = containerBuilder.Build();
			var instance = new SyncJobInLifetimeScope(_containerFactory.Object, containerWithoutSyncJob, _syncJobParameters, _relativityServices, _configuration, _logger);

			Func<Task> func = () => instance.ExecuteAsync(CompositeCancellationToken.None);

			await func.Should().ThrowAsync<SyncException>().ConfigureAwait(false);
		}
	}
}
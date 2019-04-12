using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	public class SyncJobInLifetimeScopeTests
	{
		private IContainer _container;
		private Mock<IContainerFactory> _containerFactory;
		private ISyncLog _logger;
		private SyncJobParameters _syncJobParameters;
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
			_syncJobParameters = new SyncJobParameters(1, 1);
			_configuration = new SyncJobExecutionConfiguration();
			_logger = new EmptyLogger();
		}

		[Test]
		public async Task ItShouldPassExecuteAsync()
		{
			var instance = new SyncJobInLifetimeScope(_containerFactory.Object, _container, _syncJobParameters, _configuration, _logger);

			await instance.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			_syncJob.Verify(x => x.ExecuteAsync(CancellationToken.None), Times.Once);
		}

		[Test]
		public async Task ItShouldPassExecuteWithProgressAsync()
		{
			IProgress<SyncJobState> progress = Mock.Of<IProgress<SyncJobState>>();
			var instance = new SyncJobInLifetimeScope(_containerFactory.Object, _container, _syncJobParameters, _configuration, _logger);

			await instance.ExecuteAsync(progress, CancellationToken.None).ConfigureAwait(false);

			_syncJob.Verify(x => x.ExecuteAsync(progress, CancellationToken.None), Times.Once);
		}

		[Test]
		public async Task ItShouldPassRetryAsync()
		{
			var instance = new SyncJobInLifetimeScope(_containerFactory.Object, _container, _syncJobParameters, _configuration, _logger);

			await instance.RetryAsync(CancellationToken.None).ConfigureAwait(false);

			_syncJob.Verify(x => x.RetryAsync(CancellationToken.None), Times.Once);
		}

		[Test]
		public async Task ItShouldPassRetryWithProgressAsync()
		{
			IProgress<SyncJobState> progress = Mock.Of<IProgress<SyncJobState>>();
			var instance = new SyncJobInLifetimeScope(_containerFactory.Object, _container, _syncJobParameters, _configuration, _logger);

			await instance.RetryAsync(progress, CancellationToken.None).ConfigureAwait(false);

			_syncJob.Verify(x => x.RetryAsync(progress, CancellationToken.None), Times.Once);
		}

		[Test]
		public async Task ItShouldThrowSyncException()
		{
			var containerBuilder = new ContainerBuilder();
			IContainer containerWithoutSyncJob = containerBuilder.Build();
			var instance = new SyncJobInLifetimeScope(_containerFactory.Object, containerWithoutSyncJob, _syncJobParameters, _configuration, _logger);

			Func<Task> func = async () => await instance.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			await func.Should().ThrowAsync<SyncException>().ConfigureAwait(false);
		}
	}
}
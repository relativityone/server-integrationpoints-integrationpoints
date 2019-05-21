using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	internal sealed class SynchronizationExecutorTests
	{
		private IExecutor<ISynchronizationConfiguration> _executor;
		private Mock<IObjectManager> _objectManagerMock;
		private Mock<IImportBulkArtifactJob> _importBulkArtifactJob;

		[SetUp]
		public void SetUp()
		{
			ContainerBuilder containerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
			IntegrationTestsContainerBuilder.MockStepsExcept<ISynchronizationConfiguration>(containerBuilder);
			containerBuilder.RegisterType<FakeImportJobFactory>().As<IImportJobFactory>();

			_importBulkArtifactJob = new Mock<IImportBulkArtifactJob>();
			containerBuilder.RegisterInstance(_importBulkArtifactJob.Object).As<IImportBulkArtifactJob>();

			Mock<ISemaphoreSlim> semaphoreSlim = new Mock<ISemaphoreSlim>();
			containerBuilder.RegisterInstance(semaphoreSlim.Object).As<ISemaphoreSlim>();

			_objectManagerMock = new Mock<IObjectManager>();	
			var serviceFactoryMock = new Mock<IDestinationServiceFactoryForUser>();
			var serviceFactoryMock2 = new Mock<ISourceServiceFactoryForUser>();
			serviceFactoryMock.Setup(x => x.CreateProxyAsync<IObjectManager>()).Returns(Task.FromResult(_objectManagerMock.Object));
			serviceFactoryMock2.Setup(x => x.CreateProxyAsync<IObjectManager>()).Returns(Task.FromResult(_objectManagerMock.Object));

			containerBuilder.RegisterInstance(serviceFactoryMock.Object).As<IDestinationServiceFactoryForUser>();
			containerBuilder.RegisterInstance(serviceFactoryMock2.Object).As<ISourceServiceFactoryForUser>();
			containerBuilder.RegisterType<SynchronizationExecutor>().As<IExecutor<ISynchronizationConfiguration>>();

			CorrelationId correlationId = new CorrelationId(Guid.NewGuid().ToString());

			containerBuilder.RegisterInstance(new EmptyLogger()).As<ISyncLog>();
			containerBuilder.RegisterInstance(correlationId).As<CorrelationId>();

			IContainer container = containerBuilder.Build();
			_executor = container.Resolve<IExecutor<ISynchronizationConfiguration>>();
		}

		[Test]
		public async Task ItShouldPassGoldFlow()
		{
			ConfigurationStub configuration = new ConfigurationStub();

			// act
			ExecutionResult result = await _executor.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false);

			// assert
			result.Status.Should().Be(ExecutionStatus.Completed);
		}
	}
}
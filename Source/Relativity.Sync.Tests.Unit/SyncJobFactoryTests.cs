using System;
using Autofac;
using Autofac.Core.Registration;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public sealed class SyncJobFactoryTests
	{
		private SyncJobFactory _instance;
		private Mock<IContainerFactory> _containerFactory;

		[SetUp]
		public void SetUp()
		{
			_containerFactory = new Mock<IContainerFactory>();
			_instance = new SyncJobFactory(_containerFactory.Object);
		}

		[Test]
		public void ItShouldCreateSyncJob()
		{
			SyncJobParameters syncJobParameters = new SyncJobParameters(1, 1);
			SyncJobExecutionConfiguration configuration = new SyncJobExecutionConfiguration();
			ISyncLog logger = new EmptyLogger();

			IContainer container = new ContainerBuilder().Build();

			ISyncJob expectedSyncJob = Mock.Of<ISyncJob>();
			_containerFactory.Setup(x => x.RegisterSyncDependencies(It.IsAny<ContainerBuilder>(), syncJobParameters, configuration, logger))
				.Callback((ContainerBuilder cb, SyncJobParameters p, SyncJobExecutionConfiguration c, ISyncLog l) => cb.RegisterInstance(expectedSyncJob).As<ISyncJob>());

			// ACT
			ISyncJob syncJob = _instance.Create(container, syncJobParameters, configuration, logger);

			// ASSERT
			syncJob.Should().Be(expectedSyncJob);
			_containerFactory.Verify(x => x.RegisterSyncDependencies(It.IsAny<ContainerBuilder>(), syncJobParameters, configuration, logger), Times.Once);
		}

		[Test]
		public void ItShouldRegisterDependenciesInScope()
		{
			SyncJobParameters syncJobParameters = new SyncJobParameters(1, 1);
			SyncJobExecutionConfiguration configuration = new SyncJobExecutionConfiguration();
			ISyncLog logger = new EmptyLogger();

			IContainer container = new ContainerBuilder().Build();

			_containerFactory.Setup(x => x.RegisterSyncDependencies(It.IsAny<ContainerBuilder>(), syncJobParameters, configuration, logger))
				.Callback((ContainerBuilder cb, SyncJobParameters p, SyncJobExecutionConfiguration c, ISyncLog l) => cb.RegisterInstance(Mock.Of<ISyncJob>()).As<ISyncJob>());

			// ACT
			_instance.Create(container, syncJobParameters, configuration, logger);

			Action action = () => container.Resolve<ISyncJob>();

			// ASSERT
			action.Should().Throw<ComponentNotRegisteredException>();
		}
	}
}
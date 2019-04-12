using System;
using Autofac;
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
		private Mock<IContainer> _container;
		private ISyncLog _logger;
		private SyncJobParameters _syncJobParameters;
		private SyncJobExecutionConfiguration _configuration;

		[SetUp]
		public void SetUp()
		{
			_containerFactory = new Mock<IContainerFactory>();
			_container = new Mock<IContainer>();

			_syncJobParameters = new SyncJobParameters(1, 1);
			_configuration = new SyncJobExecutionConfiguration();
			_logger = new EmptyLogger();
			_instance = new SyncJobFactory(_containerFactory.Object);
		}

		[Test]
		public void ItShouldCreateSyncJobWithAllOverrides()
		{
			_instance.Create(_container.Object, _syncJobParameters, _configuration, _logger).Should().BeOfType<SyncJobInLifetimeScope>();
			_instance.Create(_container.Object, _syncJobParameters, _configuration).Should().BeOfType<SyncJobInLifetimeScope>();
			_instance.Create(_container.Object, _syncJobParameters, _logger).Should().BeOfType<SyncJobInLifetimeScope>();
			_instance.Create(_container.Object, _syncJobParameters).Should().BeOfType<SyncJobInLifetimeScope>();
		}

		[Test]
		public void ItShouldThrowArgumentNullExceptionOnNullContainer()
		{
			Action action = () => _instance.Create(null, _syncJobParameters, _configuration, _logger);

			action.Should().Throw<ArgumentNullException>();
		}

		[Test]
		public void ItShouldThrowArgumentNullExceptionOnNullParameters()
		{
			Action action = () => _instance.Create(_container.Object, null, _configuration, _logger);

			action.Should().Throw<ArgumentNullException>();
		}

		[Test]
		public void ItShouldThrowArgumentNullExceptionOnNullConfiguration()
		{
			Action action = () => _instance.Create(_container.Object, _syncJobParameters, null, _logger);

			action.Should().Throw<ArgumentNullException>();
		}

		[Test]
		public void ItShouldThrowArgumentNullExceptionOnNullLogger()
		{
			Action action = () => _instance.Create(_container.Object, _syncJobParameters, _configuration, null);

			action.Should().Throw<ArgumentNullException>();
		}
	}
}
﻿using System;
using Autofac;
using FluentAssertions;
using kCura.WinEDDS.Service.Export;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.Logging;
using Relativity.Telemetry.APM;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public sealed class SyncJobFactoryTests
	{
		private SyncJobFactory _instance;
		private Mock<IContainer> _container;
		private ISyncLog _logger;
		private SyncJobParameters _syncJobParameters;
		private RelativityServices _relativityServices;
		private SyncJobExecutionConfiguration _configuration;

		[SetUp]
		public void SetUp()
		{
			_container = new Mock<IContainer>();

			_syncJobParameters = new SyncJobParameters(1, 1);
			_relativityServices = new RelativityServices(Mock.Of<IAPM>(), Mock.Of<IServicesMgr>(), Mock.Of<ISearchManager>,
				new Uri("http://localhost", UriKind.RelativeOrAbsolute));
			_configuration = new SyncJobExecutionConfiguration();
			_logger = new EmptyLogger();
			_instance = new SyncJobFactory(new Mock<IContainerFactory>().Object);
		}

		[Test]
		public void ItShouldCreateSyncJobWithAllOverrides()
		{
			_instance.Create(_container.Object, _syncJobParameters, _relativityServices, _configuration, _logger).Should().BeOfType<SyncJobInLifetimeScope>();
			_instance.Create(_container.Object, _syncJobParameters, _relativityServices, _configuration).Should().BeOfType<SyncJobInLifetimeScope>();
			_instance.Create(_container.Object, _syncJobParameters, _relativityServices, _logger).Should().BeOfType<SyncJobInLifetimeScope>();
			_instance.Create(_container.Object, _syncJobParameters, _relativityServices).Should().BeOfType<SyncJobInLifetimeScope>();
		}

		[Test]
		public void ItShouldThrowArgumentNullExceptionOnNullContainer()
		{
			Action action = () => _instance.Create(null, _syncJobParameters, _relativityServices, _configuration, _logger);

			action.Should().Throw<ArgumentNullException>();
		}

		[Test]
		public void ItShouldThrowArgumentNullExceptionOnNullParameters()
		{
			Action action = () => _instance.Create(_container.Object, null, _relativityServices, _configuration, _logger);

			action.Should().Throw<ArgumentNullException>();
		}

		[Test]
		public void ItShouldThrowArgumentNullExceptionOnNullConfiguration()
		{
			Action action = () => _instance.Create(_container.Object, _syncJobParameters, _relativityServices, null, _logger);

			action.Should().Throw<ArgumentNullException>();
		}

		[Test]
		public void ItShouldThrowArgumentNullExceptionOnNullLogger()
		{
			Action action = () => _instance.Create(_container.Object, _syncJobParameters, _relativityServices, _configuration, null);

			action.Should().Throw<ArgumentNullException>();
		}

		[Test]
		public void ItShouldThrowArgumentNullExceptionOnNullRelativityServices()
		{
			Action action = () => _instance.Create(_container.Object, _syncJobParameters, null, _configuration, _logger);

			action.Should().Throw<ArgumentNullException>();
		}
	}
}
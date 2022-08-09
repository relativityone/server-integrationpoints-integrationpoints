using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Sync.Logging;
using Relativity.Sync.Tests.Common;
using Relativity.Telemetry.APM;

namespace Relativity.Sync.Tests.Unit
{
    [TestFixture]
    public sealed class SyncJobFactoryTests
    {
        private SyncJobFactory _instance;
        private IAPILog _logger;
        private SyncJobParameters _syncJobParameters;
        private IRelativityServices _relativityServices;
        private SyncJobExecutionConfiguration _configuration;

        [SetUp]
        public void SetUp()
        {
            _syncJobParameters = FakeHelper.CreateSyncJobParameters();
            _relativityServices = new RelativityServices(Mock.Of<IAPM>(), new Uri("http://localhost", UriKind.RelativeOrAbsolute), Mock.Of<IHelper>());
            _configuration = new SyncJobExecutionConfiguration();
            _logger = new EmptyLogger();
            _instance = new SyncJobFactory(new Mock<IContainerFactory>().Object);
        }

        [Test]
        public void ItShouldCreateSyncJobWithAllOverrides()
        {
            _instance.Create(_syncJobParameters, _relativityServices, _configuration, _logger).Should().BeOfType<SyncJobInLifetimeScope>();
            _instance.Create(_syncJobParameters, _relativityServices, _configuration).Should().BeOfType<SyncJobInLifetimeScope>();
            _instance.Create(_syncJobParameters, _relativityServices, _logger).Should().BeOfType<SyncJobInLifetimeScope>();
            _instance.Create(_syncJobParameters, _relativityServices).Should().BeOfType<SyncJobInLifetimeScope>();
        }

        [Test]
        public void ItShouldThrowArgumentNullExceptionOnNullParameters()
        {
            Action action = () => _instance.Create(null, _relativityServices, _configuration, _logger);

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void ItShouldThrowArgumentNullExceptionOnNullConfiguration()
        {
            Action action = () => _instance.Create(_syncJobParameters, _relativityServices, null, _logger);

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void ItShouldThrowArgumentNullExceptionOnNullLogger()
        {
            Action action = () => _instance.Create(_syncJobParameters, _relativityServices, _configuration, null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void ItShouldThrowArgumentNullExceptionOnNullRelativityServices()
        {
            Action action = () => _instance.Create(_syncJobParameters, null, _configuration, _logger);

            action.Should().Throw<ArgumentNullException>();
        }
    }
}
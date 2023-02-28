using System;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.Repositories;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Services.Helpers;
using Relativity.Logging;
using Relativity.Telemetry.APM;

namespace Relativity.IntegrationPoints.Services.Tests.Managers
{
    [TestFixture, Category("Unit")]
    public class IntegrationPointHealthCheckManagerTests : TestBase
    {
        private IntegrationPointHealthCheckManager _integrationPointHealthCheckManager;
        private ILog _logger;
        private IWindsorContainer _container;
        private IAPM _apmClient;
        private IHealthMeasure _healthMeasure;
        private const int _WORKSPACE_ID = 819434;

        public override void SetUp()
        {
            _logger = Substitute.For<ILog>();
            IPermissionRepository permissionRepository = Substitute.For<IPermissionRepository>();
            _container = Substitute.For<IWindsorContainer>();

            IPermissionRepositoryFactory permissionRepositoryFactory = Substitute.For<IPermissionRepositoryFactory>();
            permissionRepositoryFactory.Create(Arg.Any<IHelper>(), _WORKSPACE_ID).Returns(permissionRepository);

            _apmClient = Substitute.For<IAPM>();
            Client.LazyAPMClient = new Lazy<IAPM>(() => _apmClient);

            _healthMeasure = Substitute.For<IHealthMeasure>();

            _apmClient.HealthCheckOperation(kCura.IntegrationPoints.Core.Constants.IntegrationPoints.Telemetry.APM_HEALTHCHECK,
                Arg.Any<Func<HealthCheckOperationResult>>()).Returns(_healthMeasure);

            _integrationPointHealthCheckManager = new IntegrationPointHealthCheckManager(_logger, permissionRepositoryFactory, _container);
        }

        [Test]
        public void ShouldWriteHealthMeasure()
        {
            // Act
            _integrationPointHealthCheckManager.RunHealthChecksAsync().Wait();

            // Assert
            _healthMeasure.Received().Write();
        }

        [Test]
        public void ShouldBeHealthyByDefault()
        {
            // Act
            HealthCheckOperationResult result = _integrationPointHealthCheckManager.RunHealthChecksAsync().Result;

            // Assert
            Assert.IsTrue(result.IsHealthy);
        }
    }
}

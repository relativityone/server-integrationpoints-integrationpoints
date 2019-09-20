﻿using System;
using System.Threading.Tasks;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Services.Helpers;
using kCura.IntegrationPoints.Services.RelativityWebApi;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;
using Relativity.Logging;
using Relativity.Telemetry.APM;

namespace kCura.IntegrationPoints.Services.Tests.Managers
{
	public class IntegrationPointHealthCheckManagerTests : TestBase
	{
		private IntegrationPointHealthCheckManager _integrationPointHealthCheckManager;
		private ILog _logger;
		private IWindsorContainer _container;
		private IAPM _apmClient;

		private const string _WEB_API_PATH_VALUE = "http://apimock.corp";
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

			_integrationPointHealthCheckManager = new IntegrationPointHealthCheckManager(_logger, permissionRepositoryFactory, _container);
		}

		[Test]
		public void ItShouldCheckIfSettingExist()
		{
			// Arrange
			SubstitureWebApiPath(string.Empty);

			// Act
			HealthCheckOperationResult result = _integrationPointHealthCheckManager.RunHealthChecksAsync().Result;

			// Assert
			Assert.IsFalse(result.IsHealthy);
			Assert.AreEqual("WebApiPath InstanceSetting is null or empty", result.Message);
		}

		[Test]
		public void ShouldReturnExceptionIfWebApiCallFails()
		{
			// Arrange
			SubstitureWebApiPath(_WEB_API_PATH_VALUE);
			RelativityManagerSoap relativityManagerSoap = SubstituteRelativityManagerSoap();
			relativityManagerSoap.GetRelativityUrlAsync().Throws(new Exception("the reason"));

			// Act
			HealthCheckOperationResult result = _integrationPointHealthCheckManager.RunHealthChecksAsync().Result;

			// Assert
			Assert.IsFalse(result.IsHealthy);
			relativityManagerSoap.Received().GetRelativityUrlAsync();
			Assert.NotNull(result.Exception);
			Assert.AreEqual($"Relativity WebApi call to '{_WEB_API_PATH_VALUE}' failed", result.Message);
			Assert.AreEqual("the reason", result.Exception.Message);
		}

		[Test]
		public void ItShouldCallWebApi()
		{
			// Arrange
			SubstitureWebApiPath(_WEB_API_PATH_VALUE);
			RelativityManagerSoap relativityManagerSoap = SubstituteRelativityManagerSoap();
			relativityManagerSoap.GetRelativityUrlAsync().Returns(Task.FromResult("mock"));

			// Act
			HealthCheckOperationResult result = _integrationPointHealthCheckManager.RunHealthChecksAsync().Result;

			// Assert
			Assert.IsTrue(result.IsHealthy);
			relativityManagerSoap.Received().GetRelativityUrlAsync();
		}

		[Test]
		public void ShouldLogSuccess()
		{
			// Arrange
			SubstitureWebApiPath(_WEB_API_PATH_VALUE);
			SubstituteRelativityManagerSoap().GetRelativityUrlAsync().Returns(Task.FromResult("mock"));

			// Act
			_integrationPointHealthCheckManager.RunHealthChecksAsync().Wait();

			// Assert
			_logger.Received().LogVerbose(Arg.Any<string>());
		}

		[Test]
		public void ShouldLogError()
		{
			// Arrange
			SubstitureWebApiPath(_WEB_API_PATH_VALUE);
			SubstituteRelativityManagerSoap().GetRelativityUrlAsync().Throws(new Exception("the reason"));

			// Act
			_integrationPointHealthCheckManager.RunHealthChecksAsync().Wait();

			// Assert
			string expectedErrorMessage = $"Relativity WebApi call to '{_WEB_API_PATH_VALUE}' failed";
			_logger.Received().LogError(Arg.Is<string>(msg => msg.Equals(expectedErrorMessage)));
		}

		[Test]
		public void ShouldWriteHealthMeasure()
		{
			// Arrange
			SubstitureWebApiPath(_WEB_API_PATH_VALUE);

			SubstituteRelativityManagerSoap().GetRelativityUrlAsync().Throws(new Exception("the reason"));

			IHealthMeasure healthMeasure = Substitute.For<IHealthMeasure>();
			_apmClient.HealthCheckOperation(Core.Constants.IntegrationPoints.Telemetry.APM_HEALTHCHECK, Arg.Any<Func<HealthCheckOperationResult>>()).Returns(healthMeasure);

			// Act
			_integrationPointHealthCheckManager.RunHealthChecksAsync().Wait();

			// Assert
			healthMeasure.Received().Write();
		}

		private RelativityManagerSoap SubstituteRelativityManagerSoap()
		{
			RelativityManagerSoap relativityManagerSoap = Substitute.For<RelativityManagerSoap>();

			IRelativityManagerSoapFactory relativityManagerSoapFactory = Substitute.For<IRelativityManagerSoapFactory>();
			relativityManagerSoapFactory.Create(Arg.Any<string>()).Returns(relativityManagerSoap);

			_container.Resolve<IRelativityManagerSoapFactory>().Returns(relativityManagerSoapFactory);
			return relativityManagerSoap;
		}

		private void SubstitureWebApiPath(string webApiPath)
		{
			IInstanceSettingRepository instanceSettingRepository = Substitute.For<IInstanceSettingRepository>();

			instanceSettingRepository.GetConfigurationValue(Domain.Constants.INTEGRATION_POINT_INSTANCE_SETTING_SECTION,
				Domain.Constants.WEB_API_PATH).Returns(webApiPath);

			IRepositoryFactory repositoryFactory = Substitute.For<IRepositoryFactory>();
			repositoryFactory.GetInstanceSettingRepository().Returns(instanceSettingRepository);

			_container.Resolve<IRepositoryFactory>().Returns(repositoryFactory);
		}
	}
}

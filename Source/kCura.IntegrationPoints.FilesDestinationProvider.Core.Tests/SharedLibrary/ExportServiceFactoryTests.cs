using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.WinEDDS.Core.Model.Export;
using kCura.WinEDDS.Service.Export;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Constants = kCura.IntegrationPoints.Domain.Constants;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.SharedLibrary
{
	[TestFixture]
	public class ExportServiceFactoryTests : TestBase
	{
		private ExportServiceFactory _instance;
		private IHelper _helper;
		private IInstanceSettingRepository _instanceSettingRepository;
		private IAPILog _logger;
		private ExportDataContext _exportDataContext;
		private CurrentUser _contextUser;

		[SetUp]
		public override void SetUp()
		{
			_logger = Substitute.For<IAPILog>();
			_helper = Substitute.For<IHelper>();
			_helper.GetLoggerFactory().GetLogger().ForContext<ExportServiceFactory>().Returns(_logger);
			_instanceSettingRepository = Substitute.For<IInstanceSettingRepository>();
			_contextUser = new CurrentUser() {ID = 9};
			_exportDataContext = new ExportDataContext() {ExportFile = new ExtendedExportFile(1234)};

			_instance = new ExportServiceFactory(_helper, _instanceSettingRepository, _contextUser);
		}

		[Test]
		[TestCase("True", typeof(CoreServiceFactory))]
		[TestCase("False", typeof(WebApiServiceFactory))]
		[TestCase("invalid boolean string", typeof(WebApiServiceFactory))]
		[TestCase("", typeof(WebApiServiceFactory))]
		public void ShouldCreateServiceFactoryBasedOnInstanceSettingValue(string useCoreApiConfig, Type type)
		{
			_instanceSettingRepository.GetConfigurationValue(Constants.INTEGRATION_POINT_INSTANCE_SETTING_SECTION,
				Constants.REPLACE_WEB_API_WITH_EXPORT_CORE).Returns(useCoreApiConfig);

			IExtendedServiceFactory result = _instance.Create(_exportDataContext);
			Assert.IsInstanceOf(type, result);
		}




	}
}
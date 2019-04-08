using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Repositories;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.WinEDDS;
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
		private IInstanceSettingRepository _instanceSettingRepository;
		private ExportDataContext _exportDataContext;

		[SetUp]
		public override void SetUp()
		{
			_instanceSettingRepository = Substitute.For<IInstanceSettingRepository>();

			IAPILog logger = Substitute.For<IAPILog>();
			logger.ForContext<ExportServiceFactory>().Returns(logger);
			IRepositoryFactory repositoryFactory = Substitute.For<IRepositoryFactory>();
			IViewFieldRepository viewFieldRepository = Substitute.For<IViewFieldRepository>();
			IFileRepository fileRepository = Substitute.For<IFileRepository>();
			IFileFieldRepository fileFieldRepository = Substitute.For<IFileFieldRepository>();
			var contextUser = new CurrentUser
			{
				ID = 9
			};
			_exportDataContext = new ExportDataContext
			{
				ExportFile = new ExtendedExportFile(1234)
			};
			
			_instance = new ExportServiceFactory(
				logger, 
				_instanceSettingRepository, 
				repositoryFactory, 
				fileRepository, 
				fileFieldRepository, 
				viewFieldRepository, 
				contextUser
			);
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

			IServiceFactory result = _instance.Create(_exportDataContext);
			Assert.IsInstanceOf(type, result);
		}
	}
}
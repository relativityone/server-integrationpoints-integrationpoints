using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Extensions;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers.FileNaming;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.Windows.Process;
using kCura.WinEDDS;
using kCura.WinEDDS.Core.Export;
using kCura.WinEDDS.Core.Export.Natives.Name.Factories;
using kCura.WinEDDS.Core.Model.Export;
using kCura.WinEDDS.Exporters;
using kCura.WinEDDS.Service.Export;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.SharedLibrary
{
	[TestFixture]
	public class FactoryConfigBuilderTests : TestBase
	{
		private FactoryConfigBuilder _factory;
		private ExportDataContext _exportDataContext;
		private JobHistoryErrorServiceProvider _jobHistoryErrorServiceProvider;
		private IHelper _helper;
		private ICaseServiceContext _context;
		private IJobHistoryErrorService _jobHistoryErrorService;
		private IInstanceSettingRepository _instanceSettingRepository;
		private IFileNameProvidersDictionaryBuilder _fileNameProvidersDictionaryBuilder;
		private IExportConfig _exportConfig;

		[SetUp]
		public override void SetUp()
		{
			_helper = Substitute.For<IHelper>();
			_exportConfig = Substitute.For<IExportConfig>();
			_context = Substitute.For<ICaseServiceContext>();
			_jobHistoryErrorService = new JobHistoryErrorService(_context, _helper);
			_jobHistoryErrorServiceProvider = new JobHistoryErrorServiceProvider(_jobHistoryErrorService);
			_instanceSettingRepository = Substitute.For<IInstanceSettingRepository>();
			_fileNameProvidersDictionaryBuilder = new FileNameProvidersDictionaryBuilder();

			_exportDataContext = new ExportDataContext
			{
				ExportFile = new ExtendedExportFile(1)
				{
					AppendOriginalFileName = true
				},
				Settings = new ExportSettings()
				{
					ArtifactTypeId = 1
				}
			};

			_factory = new FactoryConfigBuilder(_helper, _jobHistoryErrorServiceProvider, _instanceSettingRepository,
				_fileNameProvidersDictionaryBuilder, _exportConfig);
		}

		[Test]
		[TestCase("True", typeof(CoreServiceFactory))]
		[TestCase("False", typeof(WebApiServiceFactory))]
		[TestCase("invalid boolean string", typeof(WebApiServiceFactory))]
		[TestCase("", typeof(WebApiServiceFactory))]
		public void ShouldCreateCoreServiceFactoryWhenFlagSetToTrue(string useCoreApiConfig, Type type)
		{
			var config = _factory.SetupServiceFactory(_exportDataContext, useCoreApiConfig);
			Assert.IsInstanceOf(type, config);
		}

		[Test]
		public void ShouldCreateCompleteFactoryConfig()
		{
			var factoryConfig = _factory.BuildFactoryConfig(_exportDataContext);
			Assert.IsNotNull(factoryConfig.Controller);
			Assert.IsNotNull(factoryConfig.FileNameProvider);
			Assert.AreSame(factoryConfig.JobStopManager, _jobHistoryErrorServiceProvider?.JobHistoryErrorService.JobStopManager);
			Assert.IsNotNull(factoryConfig.LoadFileFormatterFactory);
			Assert.IsNotNull(factoryConfig.ServiceFactory);
			Assert.AreEqual(factoryConfig.NameTextAndNativesAfterBegBates, _exportDataContext.ExportFile.AreSettingsApplicableForProdBegBatesNameCheck());
			Assert.IsNotNull(factoryConfig.ExportConfig);
		}

	}
}

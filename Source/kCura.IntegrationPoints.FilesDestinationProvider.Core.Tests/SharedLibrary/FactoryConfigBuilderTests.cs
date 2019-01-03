using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Extensions;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers.FileNaming;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.WinEDDS.Service.Export;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using kCura.WinEDDS;

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
		private IFileNameProvidersDictionaryBuilder _fileNameProvidersDictionaryBuilder;
		private IExportConfig _exportConfig;
		private IServiceFactory _serviceFactory;

		[SetUp]
		public override void SetUp()
		{
			_helper = Substitute.For<IHelper>();
			_exportConfig = Substitute.For<IExportConfig>();
			_context = Substitute.For<ICaseServiceContext>();
			_jobHistoryErrorService = new JobHistoryErrorService(_context, _helper);
			_jobHistoryErrorServiceProvider = new JobHistoryErrorServiceProvider(_jobHistoryErrorService);

			_fileNameProvidersDictionaryBuilder = new FileNameProvidersDictionaryBuilder();
			_serviceFactory = Substitute.For<IServiceFactory>();

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

			_factory = new FactoryConfigBuilder(_jobHistoryErrorServiceProvider,_fileNameProvidersDictionaryBuilder, _exportConfig);
		}

		[Test]
		public void ShouldCreateCompleteFactoryConfig()
		{
			ExporterFactoryConfig factoryConfig = _factory.BuildFactoryConfig(_exportDataContext, _serviceFactory);
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

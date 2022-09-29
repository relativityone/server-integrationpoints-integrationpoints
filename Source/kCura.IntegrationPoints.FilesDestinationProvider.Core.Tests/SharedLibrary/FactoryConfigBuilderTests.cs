using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data.Repositories;
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
    [TestFixture, Category("Unit")]
    public class FactoryConfigBuilderTests : TestBase
    {
        private FactoryConfigBuilder _factory;
        private ExportDataContext _exportDataContext;
        private JobHistoryErrorServiceProvider _jobHistoryErrorServiceProvider;
        private IServiceFactory _serviceFactory;
        private IIntegrationPointRepository _integrationPointRepository;

        [SetUp]
        public override void SetUp()
        {
            IHelper helper = Substitute.For<IHelper>();
            IExportConfig exportConfig = Substitute.For<IExportConfig>();
            IRelativityObjectManager relativityObjectManager = Substitute.For<IRelativityObjectManager>();
            _integrationPointRepository = Substitute.For<IIntegrationPointRepository>();
            var jobHistoryErrorService = new JobHistoryErrorService(relativityObjectManager, helper, _integrationPointRepository);
            _jobHistoryErrorServiceProvider = new JobHistoryErrorServiceProvider(jobHistoryErrorService);

            IFileNameProvidersDictionaryBuilder fileNameProvidersDictionaryBuilder = new FileNameProvidersDictionaryBuilder();
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

            _factory = new FactoryConfigBuilder(_jobHistoryErrorServiceProvider,fileNameProvidersDictionaryBuilder, exportConfig);
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

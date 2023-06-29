using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport.Implementations;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;
using NSubstitute;
using NUnit.Framework;
using Relativity;
using Relativity.API;
using Relativity.DataTransfer.MessageService;

namespace kCura.IntegrationPoints.Synchronizers.RDO.Tests.JobImport
{
    [TestFixture, Category("Unit")]
    public class ImportJobFactoryTests : TestBase
    {
        private IImportAPI _importApi;
        private IDataTransferContext _transferContext;
        private ImportBulkArtifactJob _importBulkArtifactJob;
        private ImageImportBulkArtifactJob _imageImportBulkArtifactJob;
        private IHelper _helperMock;
        private IAPILog _logger;

        public override void FixtureSetUp()
        {
            base.FixtureSetUp();
            _importBulkArtifactJob = new ImportBulkArtifactJob();
            _imageImportBulkArtifactJob = new ImageImportBulkArtifactJob();
            _importApi = Substitute.For<IImportAPI>();
            _importApi.NewProductionImportJob(1).ReturnsForAnyArgs(_imageImportBulkArtifactJob);
            _importApi.NewImageImportJob().Returns(_imageImportBulkArtifactJob);
            _importApi.NewNativeDocumentImportJob().Returns(_importBulkArtifactJob);
            _transferContext = Substitute.For<IDataTransferContext>();

            _logger = Substitute.For<IAPILog>();
            _helperMock = Substitute.For<IHelper>();
            _helperMock.GetLoggerFactory().GetLogger().ForContext<NativeJobImport>().Returns(_logger);
        }

        public override void SetUp()
        {
        }

        [Test]
        public void Create_RelativityProviderProductionImportAndImageImportFlagsSetTrue_ProducesProductionImageJobImport()
        {
            var settings = new DestinationConfiguration
            {
                ArtifactTypeId = (int)ArtifactType.Document,
                ProductionImport = true,
                ImageImport = true,
                Provider = "relativity",
            };
            var factory = new ImportJobFactory(Substitute.For<IMessageService>(), _logger);

            IJobImport result = factory.Create(_importApi, new ImportSettings(settings), _transferContext, _helperMock);

            Assert.IsInstanceOf<ProductionImageJobImport>(result);
        }

        [Test]
        public void Create_RelativityProviderImageImportFlagSetTrue_ProducesImageJobImport()
        {
            var settings = new DestinationConfiguration
            {
                ArtifactTypeId = (int)ArtifactType.Document,
                ImageImport = true,
                Provider = "relativity",
            };
            var factory = new ImportJobFactory(Substitute.For<IMessageService>(), _logger);

            IJobImport result = factory.Create(_importApi, new ImportSettings(settings), _transferContext, _helperMock);

            Assert.IsInstanceOf<ImageJobImport>(result);
        }

        [TestCase(null, true)]
        [TestCase(null, false)]
        [TestCase("relativity_no_more", true)]
        [TestCase("relativity_no_more", false)]
        public void Create_NonRelativityProviderImageImportFlagSetTrue_ProducesImageJobImport(string provider, bool productionImportFlag)
        {
            var settings = new DestinationConfiguration
            {
                ArtifactTypeId = (int)ArtifactType.Document,
                ImageImport = true,
                ProductionImport = productionImportFlag,
                Provider = provider,
            };
            var factory = new ImportJobFactory(Substitute.For<IMessageService>(), _logger);

            IJobImport result = factory.Create(_importApi, new ImportSettings(settings), _transferContext, _helperMock);

            Assert.IsInstanceOf<ImageJobImport>(result);
        }

        [TestCase(null, true)]
        [TestCase(null, false)]
        [TestCase("relativity_no_more", true)]
        [TestCase("relativity_no_more", false)]
        public void Create_NonRelativityProviderImageImportFlagSetFalse_ProducesNativeJobImport(string provider, bool productionImportFlag)
        {
            var settings = new DestinationConfiguration
            {
                ArtifactTypeId = (int)ArtifactType.Document,
                ImageImport = false,
                ProductionImport = productionImportFlag,
                Provider = provider,
            };
            var factory = new ImportJobFactory(Substitute.For<IMessageService>(), _logger);

            IJobImport result = factory.Create(_importApi, new ImportSettings(settings), _transferContext, _helperMock);
            Assert.IsInstanceOf<NativeJobImport>(result);
        }

        [TestCase("relativity", false, false)]
        [TestCase("relativity", false, true)]
        [TestCase("not_relativity", false, false)]
        [TestCase("not_relativity", false, true)]
        [TestCase(null, false, false)]
        [TestCase(null, false, true)]
        public void GetJobContextType_ReturnsNative(string provider, bool imageImportFlag,
            bool productionImportFlag)
        {
            var settings = new DestinationConfiguration
            {
                ArtifactTypeId = (int)ArtifactType.Document,
                ImageImport = imageImportFlag,
                ProductionImport = productionImportFlag,
                Provider = provider,
            };

            ImportJobFactory.JobContextType result = ImportJobFactory.GetJobContextType(new ImportSettings(settings));

            Assert.AreEqual(ImportJobFactory.JobContextType.Native, result);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GetJobContextType_ReturnsImportImagesFromLoadFile(bool productionImportFlag)
        {
            var settings = new DestinationConfiguration
            {
                ArtifactTypeId = (int)ArtifactType.Document,
                ImageImport = true,
                ProductionImport = productionImportFlag,
                Provider = "not_relativity",
            };

            ImportJobFactory.JobContextType result = ImportJobFactory.GetJobContextType(new ImportSettings(settings));

            Assert.AreEqual(ImportJobFactory.JobContextType.ImportImagesFromLoadFile, result);
        }

        [Test]
        public void GetJobContextType_ReturnsRelativityToRelativityImages()
        {
            var settings = new DestinationConfiguration
            {
                ArtifactTypeId = (int)ArtifactType.Document,
                ImageImport = true,
                ProductionImport = false,
                Provider = "relativity",
            };

            ImportJobFactory.JobContextType result = ImportJobFactory.GetJobContextType(new ImportSettings(settings));

            Assert.AreEqual(ImportJobFactory.JobContextType.RelativityToRelativityImages, result);
        }

        [Test]
        public void GetJobContextType_ReturnsRelativityToRelativityImagesProduction()
        {
            var settings = new DestinationConfiguration
            {
                ArtifactTypeId = (int)ArtifactType.Document,
                ImageImport = true,
                ProductionImport = true,
                Provider = "relativity",
            };

            ImportJobFactory.JobContextType result = ImportJobFactory.GetJobContextType(new ImportSettings(settings));

            Assert.AreEqual(ImportJobFactory.JobContextType.RelativityToRelativityImagesProduction, result);
        }
    }
}

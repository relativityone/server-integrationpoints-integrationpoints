using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport.Implementations;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Synchronizers.RDO.Tests.JobImport.Implementations
{
    [TestFixture, Category("Unit")]
    public class ImageJobImportTests : TestBase
    {
        private IDataTransferContext _context;
        private IImportAPI _importApi;
        private IImportSettingsBaseBuilder<ImageSettings> _builder;
        private IAPILog _logger;
        private ImageJobImport _instance;
        private ImportSettings _importSettings;

        [SetUp]
        public override void SetUp()
        {
            _importSettings = new ImportSettings(new DestinationConfiguration());
            _importApi = Substitute.For<IImportAPI>();
            _builder = Substitute.For<IImportSettingsBaseBuilder<ImageSettings>>();
            _context = Substitute.For<IDataTransferContext>();
            _logger = Substitute.For<IAPILog>();
            IHelper helper = InitializeLoggerAndGetHelper();

            _instance = new ImageJobImport(_importSettings, _importApi, _builder, _context, helper);
        }

        [Test]
        public void ItShouldCreateJob()
        {
            // Arrange
            var expected = new ImageImportBulkArtifactJob();
            _importApi.NewImageImportJob().Returns(expected);

            // Act
            ImageImportBulkArtifactJob actual = _instance.CreateJob();

            // Assert
            Assert.AreEqual(expected, actual);
            _importApi.Received().NewImageImportJob();
        }

        private IHelper InitializeLoggerAndGetHelper()
        {
            var helper = Substitute.For<IHelper>();
            var loggerFactory = Substitute.For<ILogFactory>();
            helper.GetLoggerFactory().Returns(loggerFactory);
            loggerFactory.GetLogger().Returns(_logger);
            _logger.ForContext<ImageJobImport>().Returns(_logger);
            return helper;
        }
    }
}

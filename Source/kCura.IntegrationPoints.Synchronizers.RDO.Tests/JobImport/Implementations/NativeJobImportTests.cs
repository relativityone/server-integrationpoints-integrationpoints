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

namespace kCura.IntegrationPoints.Synchronizers.RDO.Tests.JobImport.Implementations
{
    [TestFixture, Category("Unit")]
    public class NativeJobImportTests : TestBase
    {
        private NativeJobImport _instance;
        private ImportSettings _importSettings;
        private IImportAPI _importApi;
        private IImportSettingsBaseBuilder<Settings> _builder;
        private IAPILog _loggerMock;
        private IHelper _helperMock;

        [SetUp]
        public override void SetUp()
        {
            _importSettings = Substitute.For<ImportSettings>();
            _importApi = Substitute.For<IImportAPI>();
            _builder = Substitute.For<IImportSettingsBaseBuilder<Settings>>();
            IDataTransferContext context = Substitute.For<IDataTransferContext>();
            _loggerMock = Substitute.For<IAPILog>();
            _helperMock = Substitute.For<IHelper>();
            _helperMock.GetLoggerFactory().GetLogger().ForContext<NativeJobImport>().Returns(_loggerMock);

            _instance = new NativeJobImport(_importSettings, _importApi, _builder, context, _helperMock);
        }

        [Test]
        public void ItShouldCreateJob_byNewNativeDocumentImportJob_withParams()
        {
            //Arrange
            _importSettings.ArtifactTypeId = (int) ArtifactType.Document;
            _importSettings.FederatedInstanceArtifactId = null;
            var expected = new ImportBulkArtifactJob();
            _importApi.NewNativeDocumentImportJob().Returns(expected);

            //Act
            ImportBulkArtifactJob actual = _instance.CreateJob();

            //Assert
            Assert.AreEqual(expected, actual);
            _importApi.Received().NewNativeDocumentImportJob();
        }

        [Test]
        public void ItShouldCreateJob_byNewNativeDocumentImportJob()
        {
            //Arrange
            _importSettings.ArtifactTypeId = (int)ArtifactType.Document;
            _importSettings.FederatedInstanceArtifactId = 0;
            var expected = new ImportBulkArtifactJob();
            _importApi.NewNativeDocumentImportJob().Returns(expected);

            //Act
            ImportBulkArtifactJob actual = _instance.CreateJob();

            //Assert
            Assert.AreEqual(expected, actual);
            _importApi.Received().NewNativeDocumentImportJob();
        }

        [Test]
        public void ItShouldCreateJob_byNewObjectImportJob()
        {
            //Arrange
            _importSettings.ArtifactTypeId = (int)ArtifactType.Document + 1;
            var expected = new ImportBulkArtifactJob();
            _importApi.NewObjectImportJob(Arg.Any<int>()).Returns(expected);

            //Act
            ImportBulkArtifactJob actual = _instance.CreateJob();

            //Assert
            Assert.AreEqual(expected, actual);
            _importApi.Received().NewObjectImportJob(Arg.Any<int>());
        }
    }
}

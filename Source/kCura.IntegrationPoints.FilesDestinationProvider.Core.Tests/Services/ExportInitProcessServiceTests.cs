using System.ComponentModel;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.Statistics;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Services;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using CategoryAttribute = NUnit.Framework.CategoryAttribute;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Services
{
    [TestFixture, Category("Unit")]
    internal class ExportInitProcessServiceTests : TestBase
    {
        private ExportInitProcessService _subjectUnderTests;
        private ExportUsingSavedSearchSettings _exportSettings;
        private IDocumentTotalStatistics _documentTotalStatistics;
        private IRdoStatistics _rdoStatistics;
        private IAPILog _loggerMock;

        private const int _WKSP_ID = 1;
        private const int _VIEW_ID = 2;
        private const int _FOLDER_ID = 3;
        private const int _SAVED_SEARCH_ID = 4;
        private const int _PROD_SET_ID = 5;

        //Document Type ID
        private const int _DOC_ARTIFACT_TYPE_ID = 10;

        private const int _EXPECTED_DOC_COUNT = 10;

        [SetUp]
        public override void SetUp()
        {
            _exportSettings = new ExportUsingSavedSearchSettings
            {
                SourceWorkspaceArtifactId = _WKSP_ID,
                SavedSearchArtifactId = _SAVED_SEARCH_ID,
                FolderArtifactId = _FOLDER_ID,
                ProductionId = _PROD_SET_ID,
                ViewId = _VIEW_ID,
                StartExportAtRecord = 1
            };
            IHelper helperMock = Substitute.For<IHelper>();
            _documentTotalStatistics = Substitute.For<IDocumentTotalStatistics>();
            _rdoStatistics = Substitute.For<IRdoStatistics>();
            _loggerMock = Substitute.For<IAPILog>();

            ILogFactory logFactoryMock = Substitute.For<ILogFactory>();
            IAPILog apiLog = Substitute.For<IAPILog>();

            helperMock.GetLoggerFactory().Returns(logFactoryMock);
            logFactoryMock.GetLogger().Returns(apiLog);

            apiLog.ForContext<ExportInitProcessService>().Returns(_loggerMock);

            _subjectUnderTests = new ExportInitProcessService(helperMock, _documentTotalStatistics, _rdoStatistics);
        }

        [Test]
        public void ItShouldReturnCorrectRdosCountNumber()
        {
            // Arrange
            const int artifactTypeId = 12345;
            _exportSettings.ExportType = ExportSettings.ExportType.FolderAndSubfolders.ToString();

            _rdoStatistics.ForView(artifactTypeId, _VIEW_ID).Returns(_EXPECTED_DOC_COUNT);

            // Act
            long returnedValue = _subjectUnderTests.CalculateDocumentCountToTransfer(_exportSettings, artifactTypeId);

            // Assert
            _rdoStatistics.Received().ForView(artifactTypeId, _VIEW_ID);
            Assert.That(returnedValue, Is.EqualTo(_EXPECTED_DOC_COUNT));
        }

        [Test]
        public void ItShouldReturnCorrectSavedSearchDocCountNumber()
        {
            // Arrange
            _exportSettings.ExportType = ExportSettings.ExportType.SavedSearch.ToString();

            _documentTotalStatistics.ForSavedSearch(_WKSP_ID,_SAVED_SEARCH_ID).Returns(_EXPECTED_DOC_COUNT);

            // Act
            long returnedValue = _subjectUnderTests.CalculateDocumentCountToTransfer(_exportSettings, _DOC_ARTIFACT_TYPE_ID);

            // Assert
            _documentTotalStatistics.Received().ForSavedSearch(_WKSP_ID,_SAVED_SEARCH_ID);
            Assert.That(returnedValue, Is.EqualTo(_EXPECTED_DOC_COUNT));
        }

        [Test]
        public void ItShouldReturnCorrectProductionDocCountNumber()
        {
            // Arrange
            _exportSettings.ExportType = ExportSettings.ExportType.ProductionSet.ToString();

            _documentTotalStatistics.ForProduction(_WKSP_ID,_PROD_SET_ID).Returns(_EXPECTED_DOC_COUNT);

            // Act
            long returnedValue = _subjectUnderTests.CalculateDocumentCountToTransfer(_exportSettings, _DOC_ARTIFACT_TYPE_ID);

            // Assert
            _documentTotalStatistics.Received().ForProduction(_WKSP_ID,_PROD_SET_ID);
            Assert.That(returnedValue, Is.EqualTo(_EXPECTED_DOC_COUNT));
        }

        [Test]
        [TestCase(ExportSettings.ExportType.Folder, false)]
        [TestCase(ExportSettings.ExportType.FolderAndSubfolders, true)]
        public void ItShouldReturnCorrectSavedSearchDocCountNumber(ExportSettings.ExportType exportType, bool includeSubFolders)
        {
            // Arrange
            _exportSettings.ExportType = exportType.ToString();

            _documentTotalStatistics.ForFolder(_WKSP_ID,_FOLDER_ID, _VIEW_ID, includeSubFolders).Returns(_EXPECTED_DOC_COUNT);

            // Act
            long returnedValue = _subjectUnderTests.CalculateDocumentCountToTransfer(_exportSettings, _DOC_ARTIFACT_TYPE_ID);

            // Assert
            _documentTotalStatistics.Received().ForFolder(_WKSP_ID,_FOLDER_ID, _VIEW_ID, includeSubFolders);
            Assert.That(returnedValue, Is.EqualTo(_EXPECTED_DOC_COUNT));
        }

        [Test]
        [TestCase(11, 0)]
        [TestCase(1, _EXPECTED_DOC_COUNT)]
        [TestCase(10, 1)]
        [TestCase(4, 7)]
        public void ItShouldConsiderStartIndexAtParam(int startAtIndex, int expectedCount)
        {
            // Arrange
            _exportSettings.StartExportAtRecord = startAtIndex;
            _exportSettings.ExportType = ExportSettings.ExportType.SavedSearch.ToString();

            _documentTotalStatistics.ForSavedSearch(_WKSP_ID, _SAVED_SEARCH_ID).Returns(_EXPECTED_DOC_COUNT);

            // Act
            long returnedValue = _subjectUnderTests.CalculateDocumentCountToTransfer(_exportSettings, _DOC_ARTIFACT_TYPE_ID);

            // Assert
            Assert.That(returnedValue, Is.EqualTo(expectedCount));
        }

        [Test]
        public void ItShouldThrowExeptiononUknownExportType()
        {
            // Arrange
            _exportSettings.ExportType = "SomeType";

            // Act & Assert
            InvalidEnumArgumentException thrownException = Assert.Throws<InvalidEnumArgumentException>(() =>
                _subjectUnderTests.CalculateDocumentCountToTransfer(_exportSettings, _DOC_ARTIFACT_TYPE_ID));
            _loggerMock.Received().LogError(thrownException, Arg.Any<string>(), Arg.Any<object[]>());
        }
    }
}

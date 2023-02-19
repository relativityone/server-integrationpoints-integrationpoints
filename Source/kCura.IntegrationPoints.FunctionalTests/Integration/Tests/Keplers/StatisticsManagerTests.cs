using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Tests.Integration.Helpers.WorkspaceHelpers;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Testing.Identification;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using Relativity.Services.DataContracts.DTOs.Results;
using Relativity.Services.Field;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Keplers
{
    internal class StatisticsManagerTests : TestsBase
    {
        private IStatisticsManager _sut;
        private DocumentHelper _documentHelper;
        private ProductionHelper _productionHelper;
        private SavedSearchHelper _savedSearchHelper;
        private const string _PRODUCTION_DOCUMENT_FILE_TABLE_PREFIX = "ProductionDocumentFile_";

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _documentHelper = SourceWorkspace.Helpers.DocumentHelper;
            _productionHelper = SourceWorkspace.Helpers.ProductionHelper;
            _savedSearchHelper = SourceWorkspace.Helpers.SavedSearchHelper;

            _sut = Container.Resolve<IStatisticsManager>();
        }

        [IdentifiedTest("C9A3B83C-E53B-4D25-B4A5-5B4FF7B73BE2")]
        public async Task GetDocumentsTotalForSavedSearchAsync_ShouldReturnAllDocumentsWithoutFieldsNativesAndImages()
        {
            // Arrange
            int savedSearchArtifactId = SourceWorkspace.SavedSearches.First().ArtifactId;
            IList<DocumentTest> documents = _documentHelper.GetDocumentsWithoutImagesNativesAndFields();

            // Act
            long totalDocuments = await _sut
                .GetDocumentsTotalForSavedSearchAsync(SourceWorkspace.ArtifactId, savedSearchArtifactId)
                .ConfigureAwait(false);

            // Assert
            totalDocuments.ShouldBeEquivalentTo(documents.Count);
        }

        [IdentifiedTest("386DAB5A-A60D-4CB6-92B7-2759A4DB28FB")]
        public async Task GetImagesTotalForSavedSearchAsync_ShouldProperlySumAllDocumentImages()
        {
            // Arrange
            SearchCriteria searchCriteria = new SearchCriteria(false, true, true);
            SavedSearchTest savedSearch = _savedSearchHelper.GetSavedSearchBySearchCriteria(searchCriteria);
            int totalSize = _documentHelper.GetImagesSizeForSavedSearch(savedSearch.ArtifactId);
            SetupExport(totalSize, FileType.Tif);

            // Act
            long totalImages = await _sut
                .GetImagesTotalForSavedSearchAsync(SourceWorkspace.ArtifactId, savedSearch.ArtifactId)
                .ConfigureAwait(false);

            // Assert
            totalImages.ShouldBeEquivalentTo(2*totalSize);
        }

        [IdentifiedTest("EA98BDC4-04A4-488B-AF00-260265D1F726")]
        public async Task GetImagesFileSizeForSavedSearchAsync_ShouldReturnValidFileSize()
        {
            // Arrange
            SearchCriteria searchCriteria = new SearchCriteria(false, true, false);
            SavedSearchTest savedSearch = _savedSearchHelper.GetSavedSearchBySearchCriteria(searchCriteria);
            int totalFileSize = _documentHelper.GetImagesSizeForSavedSearch(savedSearch.ArtifactId);
            SetupExport(totalFileSize, FileType.Tif);

            // Act
            long returnedTotalFileSize = await _sut
                .GetImagesFileSizeForSavedSearchAsync(SourceWorkspace.ArtifactId, savedSearch.ArtifactId)
                .ConfigureAwait(false);

            // Assert
            returnedTotalFileSize.ShouldBeEquivalentTo(totalFileSize);
        }

        [IdentifiedTest("DAF57246-9B0F-4FD0-9A56-052BD980D3F7")]
        public async Task GetNativesTotalForSavedSearchAsync_ShouldReturnOnlyOneNative()
        {
            // Arrange
            int savedSearchArtifactId = SourceWorkspace.SavedSearches.First().ArtifactId;

            // Act
            long totalNatives = await _sut
                .GetNativesTotalForSavedSearchAsync(SourceWorkspace.ArtifactId, savedSearchArtifactId)
                .ConfigureAwait(false);

            // Assert
            totalNatives.ShouldBeEquivalentTo(1);
        }

        [IdentifiedTest("23511C10-BC47-45A9-B207-8461611F159E")]
        public async Task GetNativesFileSizeForSavedSearchAsync_ShouldReturnValidFileSize()
        {
            // Arrange
            SearchCriteria searchCriteria = new SearchCriteria(true, false, false);
            SavedSearchTest savedSearch = _savedSearchHelper.GetSavedSearchBySearchCriteria(searchCriteria);
            int totalFileSize = _documentHelper.GetImagesSizeForSavedSearch(savedSearch.ArtifactId);
            SetupExport(totalFileSize, FileType.Native);

            // Act
            long returnedTotalFileSize = await _sut
                .GetNativesFileSizeForSavedSearchAsync(SourceWorkspace.ArtifactId, savedSearch.ArtifactId)
                .ConfigureAwait(false);

            // Assert
            returnedTotalFileSize.ShouldBeEquivalentTo(totalFileSize);
        }

        [IdentifiedTest("CF5F893F-FF00-4083-9C4A-8858113A7210")]
        public async Task GetDocumentsTotalForProductionAsync_ShouldReturnAllDocumentsWithoutFieldsNativesAndImages()
        {
            // Arrange
            int productionArtifactId = SourceWorkspace.Productions.First().ArtifactId;
            IList<DocumentTest> documents = _documentHelper.GetDocumentsWithoutImagesNativesAndFields();

            // Act
            long totalDocuments = await _sut
                .GetDocumentsTotalForProductionAsync(SourceWorkspace.ArtifactId, productionArtifactId)
                .ConfigureAwait(false);

            // Assert
            totalDocuments.ShouldBeEquivalentTo(documents.Count);
        }

        [IdentifiedTest("F17194A9-9C2F-4B85-8D56-72976E61BDD8")]
        public async Task GetImagesTotalForProductionAsync_ShouldProperlySumAllDocumentImages()
        {
            // Arrange
            SearchCriteria searchCriteria = new SearchCriteria(false, false, true);
            ProductionTest production = _productionHelper.GetProductionBySearchCriteria(searchCriteria);
            int totalSize = _documentHelper.GetImagesSizeForProduction(production.ArtifactId);
            SetupExport(totalSize, FileType.Tif);

            // Act
            long totalDocuments = await _sut
                .GetImagesTotalForProductionAsync(SourceWorkspace.ArtifactId, production.ArtifactId)
                .ConfigureAwait(false);

            // Assert
            totalDocuments.ShouldBeEquivalentTo(2*totalSize);
        }

        [IdentifiedTest("406519DF-5034-4856-872C-1F8A391CB084")]
        public async Task GetImagesFileSizeForProductionAsync_ShouldReturnValidFileSize()
        {
            // Arrange
            SearchCriteria searchCriteria = new SearchCriteria(false, true, false);
            ProductionTest production = _productionHelper.GetProductionBySearchCriteria(searchCriteria);
            int totalFileSize = _documentHelper.GetImagesSizeForProduction(production.ArtifactId);
            string sqlText = "SELECT COALESCE(SUM([Size]),0) FROM [{0}] AS PDF JOIN [File] AS F ON F.[FileID] = PDF.[ProducedFileID]";
            string tableName = $"{_PRODUCTION_DOCUMENT_FILE_TABLE_PREFIX}{production.ArtifactId}";
            Helper.DbContextMock.Setup(x =>
                x.ExecuteSqlStatementAsScalar<long>(It.Is<string>(parameter => parameter == string.Format(sqlText, tableName))))
                .Returns(totalFileSize);

            // Act
            long returnedTotalFileSize = await _sut
                .GetImagesFileSizeForProductionAsync(SourceWorkspace.ArtifactId, production.ArtifactId)
                .ConfigureAwait(false);

            // Assert
            returnedTotalFileSize.ShouldBeEquivalentTo(totalFileSize);
        }

        [IdentifiedTest("D8F3DFA4-6CEE-4050-8D41-79F1CEE85B6F")]
        public async Task GetNativesTotalForProductionAsync_ShouldReturnOnlyOneNative()
        {
            // Arrange
            int productionArtifactId = SourceWorkspace.Productions.First().ArtifactId;

            // Act
            long totalNatives = await _sut
                .GetNativesTotalForProductionAsync(SourceWorkspace.ArtifactId, productionArtifactId)
                .ConfigureAwait(false);

            // Assert
            totalNatives.ShouldBeEquivalentTo(1);
        }

        [IdentifiedTest("B720C668-3692-4479-911D-06B2F5AD88B2")]
        public async Task GetNativesFileSizeForProductionAsync_ShouldReturnValidFileSize()
        {
            // Arrange
            SearchCriteria searchCriteria = new SearchCriteria(true, false, true);
            ProductionTest production = _productionHelper.GetProductionBySearchCriteria(searchCriteria);
            int totalFileSize = _documentHelper.GetImagesSizeForProduction(production.ArtifactId);
            SetupExport(totalFileSize, FileType.Native);

            // Act
            long returnedTotalFileSize = await _sut
                .GetNativesFileSizeForProductionAsync(SourceWorkspace.ArtifactId, production.ArtifactId)
                .ConfigureAwait(false);

            // Assert
            returnedTotalFileSize.ShouldBeEquivalentTo(totalFileSize);
        }

        [IdentifiedTest("DD163737-CB9D-4C32-A798-FB97AFBBEDE8")]
        public async Task GetDocumentsTotalForFolderAsync_ShouldReturnAllDocumentsWithoutFieldsNativesAndImages()
        {
            // Arrange
            int folderArtifactId = SourceWorkspace.Folders.First().ArtifactId;
            IList<DocumentTest> documents = _documentHelper.GetDocumentsWithoutImagesNativesAndFields();

            // Act
            long totalDocuments = await _sut
                .GetDocumentsTotalForFolderAsync(SourceWorkspace.ArtifactId, folderArtifactId, -1, false)
                .ConfigureAwait(false);

            // Assert
            totalDocuments.ShouldBeEquivalentTo(documents.Count);
        }

        [IdentifiedTest("7443F147-132B-4DE3-B726-C90256465889")]
        public async Task GetImagesTotalForFolderAsync_ShouldProperlySumAllDocumentImages()
        {
            // Arrange
            FolderTest folder = SourceWorkspace.Folders.First();
            SearchCriteria searchCriteria = new SearchCriteria(false, false, true);
            int totalSize = _documentHelper.GetImagesSizeForFolderBySearchCriteria(folder, searchCriteria);
            SetupExport(totalSize, FileType.Tif);

            // Act
            long totalImages = await _sut
                .GetImagesTotalForFolderAsync(SourceWorkspace.ArtifactId, folder.ArtifactId, -1, false)
                .ConfigureAwait(false);

            // Assert
            totalImages.ShouldBeEquivalentTo(2*totalSize);
        }

        [IdentifiedTest("E741BE45-E954-4033-BC54-867CC2B39C67")]
        public async Task GetImagesFileSizeForFolderAsync_ShouldReturnValidFileSize()
        {
            // Arrange
            FolderTest folder = SourceWorkspace.Folders.First();
            SearchCriteria searchCriteria = new SearchCriteria(false, true, false);
            int totalFileSize = _documentHelper.GetImagesSizeForFolderBySearchCriteria(folder, searchCriteria);
            SetupExport(totalFileSize, FileType.Tif);

            // Act
            long returnedTotalFileSize = await _sut
                .GetImagesFileSizeForFolderAsync(SourceWorkspace.ArtifactId, folder.ArtifactId, -1, false)
                .ConfigureAwait(false);

            // Assert
            returnedTotalFileSize.ShouldBeEquivalentTo(totalFileSize);
        }

        [IdentifiedTest("CB9B2FA4-9C40-462D-9777-D71DC5538350")]
        public async Task GetNativesTotalForFolderAsync_ShouldReturnOnlyOneNative()
        {
            // Arrange
            int folderArtifactId = SourceWorkspace.Folders.First().ArtifactId;

            // Act
            long totalNatives = await _sut
                .GetNativesTotalForFolderAsync(SourceWorkspace.ArtifactId, folderArtifactId, -1, false)
                .ConfigureAwait(false);

            // Assert
            totalNatives.ShouldBeEquivalentTo(1);
        }

        [IdentifiedTest("5D66DB0D-A3D8-46F0-A1D7-C93174E29BE1")]
        public async Task GetNativesFileSizeForFolderAsync_ShouldReturnValidFileSize()
        {
            // Arrange
            FolderTest folder = SourceWorkspace.Folders.First();
            SearchCriteria searchCriteria = new SearchCriteria(true, false, true);
            int totalFileSize = _documentHelper.GetImagesSizeForFolderBySearchCriteria(folder, searchCriteria);
            SetupExport(totalFileSize, FileType.Native);

            // Act
            long returnedTotalFileSize = await _sut
                .GetNativesFileSizeForFolderAsync(SourceWorkspace.ArtifactId, folder.ArtifactId, -1, false)
                .ConfigureAwait(false);

            // Assert
            returnedTotalFileSize.ShouldBeEquivalentTo(totalFileSize);
        }

        private void SetupExport(int totalFileSize, FileType fileType)
        {
            List<int> artifactIds = new List<int> { ArtifactProvider.NextId(), ArtifactProvider.NextId() };
            List<object> exportResultsValues_1 = new List<object> { totalFileSize, totalFileSize, new { ArtifactID = artifactIds[0] } };
            List<object> exportResultsValues_2 = new List<object> { totalFileSize, totalFileSize, new { ArtifactID = artifactIds[1] } };
            Helper.DbContextMock.Setup(x =>
                    x.ExecuteSqlStatementAsScalar<long>(It.IsAny<string>(),
                        It.Is<SqlParameter>(parameter =>
                            parameter.TypeName == "IDs" &&
                            (string)((DataTable)parameter.Value).Rows[0].ItemArray[0] == artifactIds[0].ToString() &&
                            (string)((DataTable)parameter.Value).Rows[1].ItemArray[0] == artifactIds[1].ToString()),
                        It.Is<SqlParameter>(parameter =>
                            (FileType)parameter.Value == fileType)))
                .Returns(totalFileSize);

            Proxy.ObjectManager.Mock.Setup(x =>
                x.InitializeExportAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>()))
                    .Returns(async (int workspaceId, QueryRequest request, int start) => await Task.FromResult(new ExportInitializationResults
            {
                    RunID = new Guid("95E91649-0C15-4B8B-B813-B0266D6DA95E"),
                    RecordCount = 1,
                    FieldData = new List<FieldMetadata>
                    {
                    new FieldMetadata
                    {
                        Name = "RelativityImageCount",
                        ArtifactID = ArtifactProvider.NextId(),
                        FieldType = Relativity.Services.FieldType.FixedLengthText,
                        ViewFieldID = ArtifactProvider.NextId(),
                        Guids = new List<Guid>
                        {
                            DocumentFieldsConstants.RelativityImageCountGuid
                        }
                    },
                    new FieldMetadata
                    {
                        Name = "ImageCountFieldGuid",
                        ArtifactID = ArtifactProvider.NextId(),
                        FieldType = Relativity.Services.FieldType.FixedLengthText,
                        ViewFieldID = ArtifactProvider.NextId(),
                        Guids = new List<Guid>
                        {
                            ProductionConsts.ImageCountFieldGuid
                        }
                    },
                    new FieldMetadata
                    {
                        Name = "DocumentFieldGuid",
                        ArtifactID = ArtifactProvider.NextId(),
                        FieldType = Relativity.Services.FieldType.FixedLengthText,
                        ViewFieldID = ArtifactProvider.NextId(),
                        Guids = new List<Guid>
                        {
                            ProductionConsts.DocumentFieldGuid
                        }
                    },
                },
            }
            ));

            Proxy.ObjectManager.Mock.Setup(x =>
                    x.RetrieveResultsBlockFromExportAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<int>(),
                        It.IsAny<int>()))
                .Returns(async (int workspaceArtifactID, Guid runID, int resultsBlockSize, int exportIndexID) =>
                    await Task.FromResult(new List<RelativityObjectSlim>
            {
                new RelativityObjectSlim
                {
                    ArtifactID = artifactIds[0],
                    Values = exportResultsValues_1
                },
                new RelativityObjectSlim
                {
                    ArtifactID = artifactIds[1],
                    Values = exportResultsValues_2
                }
            }
            .ToArray()
            ));

        }
    }
}

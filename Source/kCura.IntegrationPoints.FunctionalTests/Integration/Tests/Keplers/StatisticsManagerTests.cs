using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Tests.Integration.Helpers.WorkspaceHelpers;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Testing.Identification;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Keplers
{
    internal class StatisticsManagerTests : TestsBase
    {
        private IStatisticsManager _sut;
        private DocumentHelper _documentHelper;
        private ProductionHelper _productionHelper;
        private SavedSearchHelper _savedSearchHelper;

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
            int savedSearchArtifactId = SourceWorkspace.SavedSearches.First().ArtifactId;

            // Act 
            long totalImages = await _sut
                .GetImagesTotalForSavedSearchAsync(SourceWorkspace.ArtifactId, savedSearchArtifactId)
                .ConfigureAwait(false);

            // Assert
            totalImages.ShouldBeEquivalentTo(5555);
        }

        [IdentifiedTest("EA98BDC4-04A4-488B-AF00-260265D1F726")]
        public async Task GetImagesFileSizeForSavedSearchAsync_ShouldReturnValidFileSize()
        {
            // Arrange
            SearchCriteria searchCriteria = new SearchCriteria(false, true, false);
            SavedSearchTest savedSearch = _savedSearchHelper.GetSavedSearchBySearchCriteria(searchCriteria);
            int totalFileSize = _documentHelper.GetImagesSizeForSavedSearch(savedSearch.ArtifactId);
            Helper.DbContextMock.Setup(x =>
                    x.ExecuteSqlStatementAsScalar<long>(It.IsAny<string>(), It.IsAny<SqlParameter>(),
                        It.IsAny<SqlParameter>()))
                .Returns(totalFileSize);

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
            Helper.DbContextMock.Setup(x =>
                    x.ExecuteSqlStatementAsScalar<long>(It.IsAny<string>(), It.IsAny<SqlParameter>(),
                        It.IsAny<SqlParameter>()))
                .Returns(totalFileSize);

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
            int productionArtifactId = SourceWorkspace.Productions.First().ArtifactId;

            // Act 
            long totalDocuments = await _sut
                .GetImagesTotalForProductionAsync(SourceWorkspace.ArtifactId, productionArtifactId)
                .ConfigureAwait(false);

            // Assert
            totalDocuments.ShouldBeEquivalentTo(3333);
        }

        [IdentifiedTest("406519DF-5034-4856-872C-1F8A391CB084")]
        public async Task GetImagesFileSizeForProductionAsync_ShouldReturnValidFileSize()
        {
            // Arrange
            SearchCriteria searchCriteria = new SearchCriteria(false, true, false);
            ProductionTest production = _productionHelper.GetProductionBySearchCriteria(searchCriteria);
            int totalFileSize = _documentHelper.GetImagesSizeForProduction(production.ArtifactId);
            Helper.DbContextMock.Setup(x =>
                    x.ExecuteSqlStatementAsScalar<long>(It.IsAny<string>()))
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
            Helper.DbContextMock.Setup(x =>
                    x.ExecuteSqlStatementAsScalar<long>(It.IsAny<string>(), It.IsAny<SqlParameter[]>()))
                .Returns(totalFileSize);

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
            int folderArtifactId = SourceWorkspace.Folders.First().ArtifactId;

            // Act 
            long totalImages = await _sut
                .GetImagesTotalForFolderAsync(SourceWorkspace.ArtifactId, folderArtifactId, -1, false)
                .ConfigureAwait(false);

            // Assert
            totalImages.ShouldBeEquivalentTo(5555);
        }

        [IdentifiedTest("E741BE45-E954-4033-BC54-867CC2B39C67")]
        public async Task GetImagesFileSizeForFolderAsync_ShouldReturnValidFileSize()
        {
            // Arrange
            FolderTest folder = SourceWorkspace.Folders.First();
            SearchCriteria searchCriteria = new SearchCriteria(false, true, false);
            int totalFileSize = _documentHelper.GetImagesSizeForFolderBySearchCriteria(folder, searchCriteria);

            Helper.DbContextMock.Setup(x =>
                    x.ExecuteSqlStatementAsScalar<long>(It.IsAny<string>(), It.IsAny<SqlParameter>(),
                        It.IsAny<SqlParameter>()))
                .Returns(totalFileSize);

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
            Helper.DbContextMock.Setup(x =>
                    x.ExecuteSqlStatementAsScalar<long>(It.IsAny<string>(), It.IsAny<SqlParameter>(),
                        It.IsAny<SqlParameter>()))
                .Returns(totalFileSize);

            // Act 
            long returnedTotalFileSize = await _sut
                .GetNativesFileSizeForFolderAsync(SourceWorkspace.ArtifactId, folder.ArtifactId, -1, false)
                .ConfigureAwait(false);

            // Assert
            returnedTotalFileSize.ShouldBeEquivalentTo(totalFileSize);
        }
    }
}

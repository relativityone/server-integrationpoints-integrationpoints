using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Tests.Integration.Helpers.WorkspaceHelpers;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Keplers
{
    internal class StatisticsManagerTests : TestsBase
    {
        private IStatisticsManager _sut;
        private DocumentHelper _documentHelper;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _documentHelper = SourceWorkspace.Helpers.DocumentHelper;
            Container.Register(Component.For<IServicesMgr>().UsingFactoryMethod(() => Helper.GetServicesManager()).IsDefault().Named("ServicesManager"));
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
            long totalDocuments = await _sut
                .GetImagesTotalForSavedSearchAsync(SourceWorkspace.ArtifactId, savedSearchArtifactId)
                .ConfigureAwait(false);

            // Assert
            totalDocuments.ShouldBeEquivalentTo(5555);
        }

        [IdentifiedTest("EA98BDC4-04A4-488B-AF00-260265D1F726")]
        public async Task GetImagesFileSizeForSavedSearchAsync_ShouldReturnValidFileSize()
        {
            // Arrange
            int totalFileSize = 4444;
            int savedSearchArtifactId = SourceWorkspace.SavedSearches.First().ArtifactId;
            Helper.DbContextMock.Setup(x =>
                    x.ExecuteSqlStatementAsScalar<long>(It.IsAny<string>(), It.IsAny<SqlParameter>(),
                        It.IsAny<SqlParameter>()))
                .Returns(totalFileSize);

            // Act 
            long returnedTotalFileSize = await _sut
                .GetImagesFileSizeForSavedSearchAsync(SourceWorkspace.ArtifactId, savedSearchArtifactId)
                .ConfigureAwait(false);

            // Assert
            returnedTotalFileSize.ShouldBeEquivalentTo(totalFileSize);
        }

        [IdentifiedTest("CF5F893F-FF00-4083-9C4A-8858113A7210")]
        public async Task GetDocumentsTotalForProductionAsync_ShouldReturnAllDocumentsWithoutFieldsNativesAndImages()
        {
            // Arrange
            int savedSearchArtifactId = SourceWorkspace.SavedSearches.First().ArtifactId;
            IList<DocumentTest> documents = _documentHelper.GetDocumentsWithoutImagesNativesAndFields();

            // Act 
            long totalDocuments = await _sut
                .GetDocumentsTotalForProductionAsync(SourceWorkspace.ArtifactId, savedSearchArtifactId)
                .ConfigureAwait(false);

            // Assert
            totalDocuments.ShouldBeEquivalentTo(documents.Count);
        }
    }
}

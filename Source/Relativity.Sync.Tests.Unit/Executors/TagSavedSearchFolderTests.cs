using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.Search;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Tests.Unit.Executors
{
    [TestFixture]
    public sealed class TagSavedSearchFolderTests
    {
        private Mock<IAPILog> _syncLogMock;
        private Mock<IDestinationServiceFactoryForUser> _serviceFactoryForUser;
        private Mock<ISearchContainerManager> _searchContainerManager;
        private TagSavedSearchFolder _instance;

        private const int _SEARCH_CONTAINER_ARTIFACT_ID = 123456;
        private const int _WORKSPACE_ARTIFACT_ID = 345678;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _syncLogMock = new Mock<IAPILog>();
        }

        [SetUp]
        public void SetUp()
        {
            _serviceFactoryForUser = new Mock<IDestinationServiceFactoryForUser>();
            _searchContainerManager = new Mock<ISearchContainerManager>();

            _serviceFactoryForUser
                .Setup(x => x.CreateProxyAsync<ISearchContainerManager>())
                .ReturnsAsync(_searchContainerManager.Object);

            _instance = new TagSavedSearchFolder(_serviceFactoryForUser.Object, _syncLogMock.Object);
        }

        [Test]
        public async Task ItShouldReturnExistingFolder()
        {
            // ARRANGE
            var searchContainer = new SearchContainer()
            {
                ArtifactID = _SEARCH_CONTAINER_ARTIFACT_ID
            };
            var result = new SearchContainerQueryResultSet()
            {
                Success = true,
                Results = new List<Result<SearchContainer>>() { new Result<SearchContainer>() { Artifact = searchContainer } }
            };

            _searchContainerManager
                .Setup(x => x.QueryAsync(
                    It.Is<int>(y => y == _WORKSPACE_ARTIFACT_ID),
                    It.Is<Services.Query>(y => VerifyQuery(y))
                    )).ReturnsAsync(result);

            // ACT
            int folderId = await _instance.GetFolderIdAsync(_WORKSPACE_ARTIFACT_ID).ConfigureAwait(false);

            // ASSERT
            Assert.AreEqual(_SEARCH_CONTAINER_ARTIFACT_ID, folderId);
            _searchContainerManager.Verify(x => x.CreateSingleAsync(It.IsAny<int>(), It.IsAny<SearchContainer>()), Times.Never);
        }

        [Test]
        public async Task ItShouldCreateFolderIfNotFound()
        {
            // ARRANGE
            var result = new SearchContainerQueryResultSet()
            {
                Success = true,
                Results = new List<Result<SearchContainer>>()
            };

            _searchContainerManager
                .Setup(x => x.QueryAsync(
                    It.Is<int>(y => y == _WORKSPACE_ARTIFACT_ID),
                    It.Is<Services.Query>(y => VerifyQuery(y))
                    )).ReturnsAsync(result);

            _searchContainerManager
                .Setup(x => x.CreateSingleAsync(It.Is<int>(y => y == _WORKSPACE_ARTIFACT_ID), It.IsAny<SearchContainer>()))
                .ReturnsAsync(_SEARCH_CONTAINER_ARTIFACT_ID);

            // ACT
            int folderId = await _instance.GetFolderIdAsync(_WORKSPACE_ARTIFACT_ID).ConfigureAwait(false);

            // ASSERT
            Assert.AreEqual(_SEARCH_CONTAINER_ARTIFACT_ID, folderId);
        }

        private bool VerifyQuery(Services.Query query)
        {
            string queryString = "'Name' == 'Integration Points'";
            string fieldIdentifierName = "ArtifactID";
            SortEnum sortDirection = SortEnum.Descending;

            Sort sort = query.Sorts.First();

            return query.Sorts.Count == 1
                    && query.Condition.Equals(queryString, StringComparison.InvariantCulture)
                    && sort.FieldIdentifier.Name.Equals(fieldIdentifierName, StringComparison.InvariantCulture)
                    && sort.Direction == sortDirection;
        }

        [Test]
        public void ItShouldThrowExceptionOnNonSuccessfulQuery()
        {
            // ARRANGE
            var result = new SearchContainerQueryResultSet()
            {
                Success = false
            };

            _searchContainerManager
                .Setup(x => x.QueryAsync(It.Is<int>(y => y == _WORKSPACE_ARTIFACT_ID), It.IsAny<Services.Query>()))
                .ReturnsAsync(result);

            // ACT & ASSERT
            Assert.ThrowsAsync<SyncException>(async () => await _instance.GetFolderIdAsync(_WORKSPACE_ARTIFACT_ID).ConfigureAwait(false));

            _searchContainerManager.Verify(x => x.CreateSingleAsync(It.IsAny<int>(), It.IsAny<SearchContainer>()), Times.Never);
        }

        [Test]
        public void ItShouldThrowExceptionOnCreationFailure()
        {
            // ARRANGE
            var result = new SearchContainerQueryResultSet()
            {
                Success = true,
                Results = new List<Result<SearchContainer>>()
            };

            _searchContainerManager
                .Setup(x => x.QueryAsync(It.Is<int>(y => y == _WORKSPACE_ARTIFACT_ID), It.IsAny<Services.Query>()))
                .ReturnsAsync(result);

            _searchContainerManager
                .Setup(x => x.CreateSingleAsync(It.Is<int>(y => y == _WORKSPACE_ARTIFACT_ID), It.IsAny<SearchContainer>()))
                .Throws<InvalidOperationException>();

            // ACT & ASSERT
            Assert.ThrowsAsync<SyncException>(async () => await _instance.GetFolderIdAsync(_WORKSPACE_ARTIFACT_ID).ConfigureAwait(false));
        }

        [Test]
        public void ItShouldThrowExceptionOnQueryFailure()
        {
            // ARRANGE
            _searchContainerManager
                .Setup(x => x.QueryAsync(It.Is<int>(y => y == _WORKSPACE_ARTIFACT_ID), It.IsAny<Services.Query>()))
                .Throws<InvalidOperationException>();

            // ACT & ASSERT
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _instance.GetFolderIdAsync(_WORKSPACE_ARTIFACT_ID).ConfigureAwait(false));

            _searchContainerManager.Verify(x => x.CreateSingleAsync(It.IsAny<int>(), It.IsAny<SearchContainer>()), Times.Never);
        }

        [Test]
        public void ItShouldThrowExceptionOnCreateProxyFailure()
        {
            // ARRANGE
            _serviceFactoryForUser
                .Setup(x => x.CreateProxyAsync<ISearchContainerManager>())
                .Throws<InvalidOperationException>();

            // ACT & ASSERT
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _instance.GetFolderIdAsync(_WORKSPACE_ARTIFACT_ID).ConfigureAwait(false));
        }
    }
}

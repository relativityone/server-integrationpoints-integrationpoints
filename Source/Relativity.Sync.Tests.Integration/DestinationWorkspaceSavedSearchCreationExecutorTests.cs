using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.Search;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.Integration.Helpers;

namespace Relativity.Sync.Tests.Integration
{
    [TestFixture]
    public class DestinationWorkspaceSavedSearchCreationExecutorTests
    {
        private Mock<ISearchContainerManager> _searchContainerManager;
        private Mock<IKeywordSearchManager> _keywordSearchManager;
        private IExecutor<IDestinationWorkspaceSavedSearchCreationConfiguration> _executor;

        private const string _SAVED_SEARCH_FOLDER_NAME = "Integration Points";
        private readonly Guid _jobHistoryFieldOnDocumentGuid = new Guid("7cc3faaf-cbb8-4315-a79f-3aa882f1997f");
        private readonly Guid _fileIconGuid = new Guid("861295b5-5b1d-4830-89e7-77e0a7ef1c30");
        private readonly Guid _controlNumberGuid = new Guid("2a3f1212-c8ca-4fa9-ad6b-f76c97f05438");

        [SetUp]
        public void SetUp()
        {
            ContainerBuilder containerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
            IntegrationTestsContainerBuilder.MockStepsExcept<IDestinationWorkspaceSavedSearchCreationConfiguration>(containerBuilder);

            _searchContainerManager = new Mock<ISearchContainerManager>();
            _keywordSearchManager = new Mock<IKeywordSearchManager>();
            var serviceFactoryMock = new Mock<IDestinationServiceFactoryForUser>();
            serviceFactoryMock.Setup(x => x.CreateProxyAsync<ISearchContainerManager>()).ReturnsAsync(_searchContainerManager.Object);
            serviceFactoryMock.Setup(x => x.CreateProxyAsync<IKeywordSearchManager>()).ReturnsAsync(_keywordSearchManager.Object);

            containerBuilder.RegisterInstance(serviceFactoryMock.Object).As<IDestinationServiceFactoryForUser>();
            containerBuilder.RegisterType<DestinationWorkspaceSavedSearchCreationExecutor>().As<IExecutor<IDestinationWorkspaceSavedSearchCreationConfiguration>>();

            containerBuilder.RegisterInstance(new EmptyLogger()).As<IAPILog>();

            IContainer container = containerBuilder.Build();
            _executor = container.Resolve<IExecutor<IDestinationWorkspaceSavedSearchCreationConfiguration>>();
        }

        [Test]
        public async Task ItShouldReturnFailedResultWhenSavedSearchFolderFails()
        {
            SearchContainerQueryResultSet queryResult = new SearchContainerQueryResultSet
            {
                Success = false
            };
            _searchContainerManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.Is<Services.Query>(q => VerifyQuery(q)))).ReturnsAsync(queryResult);
            var configuration = new ConfigurationStub();

            // act
            ExecutionResult result = await _executor.ExecuteAsync(configuration, CompositeCancellationToken.None).ConfigureAwait(false);

            // assert
            result.Exception.Should().BeOfType<SyncException>();
            result.Status.Should().Be(ExecutionStatus.Failed);
        }

        [Test]
        public async Task ItShouldReturnFailedResultWhenSavedSearchFolderCreationFails()
        {
            SearchContainerQueryResultSet queryResult = new SearchContainerQueryResultSet
            {
                Success = true,
                Results = new List<Result<SearchContainer>>()
            };
            _searchContainerManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.Is<Services.Query>(q => VerifyQuery(q)))).ReturnsAsync(queryResult);
            _searchContainerManager.Setup(x => x.CreateSingleAsync(It.IsAny<int>(), It.IsAny<SearchContainer>())).Throws<InvalidOperationException>();
            var configuration = new ConfigurationStub();

            // act
            ExecutionResult result = await _executor.ExecuteAsync(configuration, CompositeCancellationToken.None).ConfigureAwait(false);

            // assert
            result.Exception.Should().BeOfType<SyncException>().Which.InnerException.Should().BeOfType<InvalidOperationException>();
            result.Status.Should().Be(ExecutionStatus.Failed);
        }

        [Test]
        public async Task ItShouldReturnFailedResultWhenCreatingSavedSearchFails()
        {
            const int folderArtifactId = 1;

            SearchContainerQueryResultSet queryResult = new SearchContainerQueryResultSet
            {
                Success = true,
                Results = new List<Result<SearchContainer>>()
                {
                    new Result<SearchContainer>()
                    {
                        Artifact = new SearchContainer()
                        {
                            ArtifactID = folderArtifactId
                        }
                    }
                }
            };
            _searchContainerManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.Is<Services.Query>(q => VerifyQuery(q)))).ReturnsAsync(queryResult);
            _keywordSearchManager.Setup(x => x.CreateSingleAsync(It.IsAny<int>(), It.IsAny<KeywordSearch>())).Throws<InvalidOperationException>();
            var configuration = new ConfigurationStub();
            configuration.SetSourceJobTagName("Some name");

            // act
            ExecutionResult result = await _executor.ExecuteAsync(configuration, CompositeCancellationToken.None).ConfigureAwait(false);

            // assert
            result.Exception.Should().BeOfType<SyncException>().Which.InnerException.Should().BeOfType<InvalidOperationException>();
            result.Status.Should().Be(ExecutionStatus.Failed);
        }

        [Test]
        public async Task ItShouldSetSavedSearchArtifactIdInConfiguration()
        {
            const int folderArtifactId = 1;
            const int savedSearchId = 2;

            SearchContainerQueryResultSet queryResult = new SearchContainerQueryResultSet
            {
                Success = true,
                Results = new List<Result<SearchContainer>>()
                {
                    new Result<SearchContainer>()
                    {
                        Artifact = new SearchContainer()
                        {
                            ArtifactID = folderArtifactId
                        }
                    }
                }
            };
            _searchContainerManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.Is<Services.Query>(q => VerifyQuery(q)))).ReturnsAsync(queryResult);
            _keywordSearchManager.Setup(x => x.CreateSingleAsync(It.IsAny<int>(), It.Is<KeywordSearch>(k => VerifyKeywordSearch(k, folderArtifactId)))).ReturnsAsync(savedSearchId);
            var configuration = new ConfigurationStub();
            configuration.SetSourceJobTagName("Some name");

            // act
            ExecutionResult result = await _executor.ExecuteAsync(configuration, CompositeCancellationToken.None).ConfigureAwait(false);

            // assert
            result.Status.Should().Be(ExecutionStatus.Completed);
            configuration.SavedSearchArtifactId.Should().Be(savedSearchId);
            configuration.IsSavedSearchArtifactIdSet.Should().BeTrue();
        }

        private bool VerifyQuery(Services.Query query)
        {
            return query.Condition.Contains(_SAVED_SEARCH_FOLDER_NAME) &&
                    query.Sorts.First().FieldIdentifier.Name == "ArtifactID";
        }

        private bool VerifyKeywordSearch(KeywordSearch keywordSearch, int savedSearchFolderId)
        {
            Criteria criteria = keywordSearch.SearchCriteria.Conditions.FirstOrDefault() as Criteria;
            if (criteria == null)
            {
                return false;
            }

            const int expectedNumberOfFields = 3;

            return
                criteria.Condition.FieldIdentifier.Guids.Contains(_jobHistoryFieldOnDocumentGuid) &&
                keywordSearch.ArtifactTypeID == (int)ArtifactType.Document &&
                keywordSearch.SearchContainer.ArtifactID == savedSearchFolderId &&
                keywordSearch.Fields.Count == expectedNumberOfFields &&
                keywordSearch.Fields.Exists(f => f.Guids.Contains(_fileIconGuid)) &&
                keywordSearch.Fields.Exists(f => f.Guids.Contains(_controlNumberGuid)) &&
                keywordSearch.Fields.Exists(f => f.Name == "Edit");
        }
    }
}

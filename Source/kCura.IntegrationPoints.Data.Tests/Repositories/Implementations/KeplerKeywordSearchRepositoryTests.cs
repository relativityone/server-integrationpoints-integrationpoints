using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.Search;

namespace kCura.IntegrationPoints.Data.Tests.Repositories.Implementations
{
    [TestFixture, Category("Unit")]
    public class KeplerKeywordSearchRepositoryTests : TestBase
    {
        private const int _WORKSPACE_ID = 118503;
        private IKeywordSearchManager _keywordSearchManager;
        private ISearchContainerManager _searchContainerManager;
        private KeplerKeywordSearchRepository _instance;

        public override void SetUp()
        {
            _keywordSearchManager = Substitute.For<IKeywordSearchManager>();
            _searchContainerManager = Substitute.For<ISearchContainerManager>();

            var servicesMgr = Substitute.For<IServicesMgr>();
            servicesMgr.CreateProxy<IKeywordSearchManager>(ExecutionIdentity.CurrentUser).Returns(_keywordSearchManager);
            servicesMgr.CreateProxy<ISearchContainerManager>(ExecutionIdentity.CurrentUser).Returns(_searchContainerManager);

            _instance = new KeplerKeywordSearchRepository(servicesMgr);
        }

        [Test]
        public void ItShouldCreateSavedSearch()
        {
            int expectedResult = 198911;

            var searchDto = new KeywordSearch();

            _keywordSearchManager.CreateSingleAsync(_WORKSPACE_ID, searchDto).Returns(Task.FromResult(expectedResult));

            var actualResult = _instance.CreateSavedSearch(_WORKSPACE_ID, searchDto);

            Assert.That(actualResult, Is.EqualTo(expectedResult));
            _keywordSearchManager.Received(1).CreateSingleAsync(_WORKSPACE_ID, searchDto);
        }

        [Test]
        public void ItShouldCreateSearchContainerInRoot()
        {
            int expectedResult = 155983;

            string name = "name_595";

            _searchContainerManager
                .CreateSingleAsync(_WORKSPACE_ID, Arg.Is<SearchContainer>(x => x.Name == name && x.ParentSearchContainer.ArtifactID == 0))
                .Returns(Task.FromResult(expectedResult));

            var actualResult = _instance.CreateSearchContainerInRoot(_WORKSPACE_ID, name);

            Assert.That(actualResult, Is.EqualTo(expectedResult));
            _searchContainerManager
                .Received(1)
                .CreateSingleAsync(_WORKSPACE_ID, Arg.Is<SearchContainer>(x => x.Name == name && x.ParentSearchContainer.ArtifactID == 0));
        }

        [Test]
        public void ItShouldQueryExistingSearchContainer()
        {
            var searchContainer = new SearchContainer();

            string name = "name_595";

            _searchContainerManager
                .QueryAsync(_WORKSPACE_ID, Arg.Is<Query>(x => x.Condition == GetQuerySearchContainerCondition(name)))
                .Returns(Task.FromResult(CreateSearchContainerResult(searchContainer)));

            var actualResult = _instance.QuerySearchContainer(_WORKSPACE_ID, name);

            Assert.That(actualResult, Is.EqualTo(searchContainer));
            _searchContainerManager
                .Received(1)
                .QueryAsync(_WORKSPACE_ID, Arg.Is<Query>(x => x.Condition == GetQuerySearchContainerCondition(name)));
        }

        [Test]
        public void ItShouldHandleQueryingNonExistingSearchContainer()
        {
            string name = "name_494";

            _searchContainerManager
                .QueryAsync(_WORKSPACE_ID, Arg.Is<Query>(x => x.Condition == GetQuerySearchContainerCondition(name)))
                .Returns(Task.FromResult(new SearchContainerQueryResultSet
                {
                    Success = true,
                    Results = new List<Result<SearchContainer>>()
                }));

            var actualResult = _instance.QuerySearchContainer(_WORKSPACE_ID, name);

            Assert.That(actualResult, Is.Null);
            _searchContainerManager
                .Received(1)
                .QueryAsync(_WORKSPACE_ID, Arg.Is<Query>(x => x.Condition == GetQuerySearchContainerCondition(name)));
        }

        [Test]
        public void ItShouldHandleFailureWhenQueryingSearchContainer()
        {
            string name = "name_999";

            _searchContainerManager
                .QueryAsync(_WORKSPACE_ID, Arg.Is<Query>(x => x.Condition == GetQuerySearchContainerCondition(name)))
                .Returns(Task.FromResult(new SearchContainerQueryResultSet
                {
                    Success = false
                }));

            Assert.That(() => _instance.QuerySearchContainer(_WORKSPACE_ID, name), Throws.Exception);

            _searchContainerManager
                .Received(1)
                .QueryAsync(_WORKSPACE_ID, Arg.Is<Query>(x => x.Condition == GetQuerySearchContainerCondition(name)));
        }

        private static string GetQuerySearchContainerCondition(string name)
        {
            Condition condition = new TextCondition(ClientFieldNames.Name, TextConditionEnum.EqualTo, name);
            return condition.ToQueryString();
        }

        private static SearchContainerQueryResultSet CreateSearchContainerResult(SearchContainer searchContainer)
        {
            return new SearchContainerQueryResultSet
            {
                Results = new List<Result<SearchContainer>>
                {
                    new Result<SearchContainer>
                    {
                        Artifact = searchContainer
                    }
                },
                Success = true
            };
        }
    }
}

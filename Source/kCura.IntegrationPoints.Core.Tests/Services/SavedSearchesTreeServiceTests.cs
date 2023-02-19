using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Tests.Helpers;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Search;

namespace kCura.IntegrationPoints.Core.Tests.Services
{
    [TestFixture, Category("Unit")]
    public class SavedSearchesTreeServiceTests : TestBase
    {
        private readonly int workspaceArtifactId = 1042;

        private ISearchContainerManager _searchContainerManager;
        private ISavedSearchQueryRepository _savedSearchQueryRepository;
        private ISavedSearchesTreeCreator _treeCreator;
        private SavedSearchesTreeService _subjectUnderTest;

        [SetUp]
        public override void SetUp()
        {
            _savedSearchQueryRepository = Substitute.For<ISavedSearchQueryRepository>();
            _searchContainerManager = Substitute.For<ISearchContainerManager>();
            _treeCreator = Substitute.For<ISavedSearchesTreeCreator>();

            IRepositoryFactory repoFactoryMock = Substitute.For<IRepositoryFactory>();
            repoFactoryMock.GetSavedSearchQueryRepository(workspaceArtifactId).Returns(_savedSearchQueryRepository);
            IServicesMgr servicesManager = Substitute.For<IServicesMgr>();
            servicesManager.CreateProxy<ISearchContainerManager>(Arg.Any<ExecutionIdentity>())
                .Returns(_searchContainerManager);
            IHelper helper = Substitute.For<IHelper>();
            helper.GetServicesManager()
                .Returns(servicesManager);

            _subjectUnderTest = new SavedSearchesTreeService(helper, _treeCreator, repoFactoryMock);
        }

        [Test]
        public async Task ItShouldReturnSavedSearchesTree()
        {
            // arrange
            _searchContainerManager.GetSearchContainerTreeAsync(workspaceArtifactId,
                Arg.Is<List<int>>(list => !list.Any()))
                .Returns(SavedSearchesTreeTestHelper.GetSampleContainerCollection());

            _treeCreator.Create(
                    Arg.Any<IEnumerable<SearchContainerItem>>(),
                    Arg.Any<IEnumerable<SavedSearchContainerItem>>())
                .Returns(SavedSearchesTreeTestHelper.GetSampleTreeWithSearches());

            // act
            var actual = await _subjectUnderTest.GetSavedSearchesTreeAsync(workspaceArtifactId).ConfigureAwait(false);

            // assert
            Assert.That(actual, Is.Not.Null);

            var expected = SavedSearchesTreeTestHelper.GetSampleTreeWithSearches();
            Assert.That(actual.Id, Is.EqualTo(expected.Id));
            Assert.That(actual.Text, Is.EqualTo(expected.Text));
            Assert.That(actual.Children.Count, Is.EqualTo(expected.Children.Count));

            // check mock arguments
            _treeCreator.Received(1).Create(
                    Arg.Is<IEnumerable<SearchContainerItem>>(list => list.Count() == SavedSearchesTreeTestHelper.GetSampleContainerCollection().SearchContainerItems.Count),
                    Arg.Is<IEnumerable<SavedSearchContainerItem>>(list => list.Count() != SavedSearchesTreeTestHelper.GetSampleContainerCollection().SavedSearchContainerItems.Count));

            var publicSearches = SavedSearchesTreeTestHelper.GetSampleContainerCollection().SavedSearchContainerItems
                .Where(s => !s.Secured);

            _treeCreator.Received(1).Create(
                Arg.Is<IEnumerable<SearchContainerItem>>(list => list.Count() == SavedSearchesTreeTestHelper.GetSampleContainerCollection().SearchContainerItems.Count),
                Arg.Is<IEnumerable<SavedSearchContainerItem>>(list => list.Count() == publicSearches.Count()));
        }

        [Test]
        public async Task GetTreeWithVisibleSavedSearch_ItShouldQuerySearchContainerTreeWithProperParameters()
        {
            int savedSearchId = 6543;
            List<int> savedSearchContainerAncestors = new List<int> { 2242, 645342 };
            int[] ancestorIds = savedSearchContainerAncestors.Concat(new[] { 63543 }).ToArray();
            InitializeSavedSearchAncestors(savedSearchId, ancestorIds);

            var searchContainerItemCollection = new SearchContainerItemCollection();
            searchContainerItemCollection.SearchContainerItems = new List<SearchContainerItem>
            {
                new SearchContainerItem()
            };
            searchContainerItemCollection.SavedSearchContainerItems = new List<SavedSearchContainerItem>
            {
                new SavedSearchContainerItem {Secured = false}
            };
            _searchContainerManager.GetSearchContainerTreeAsync(workspaceArtifactId, Arg.Is<List<int>>(x => x.SequenceEqual(savedSearchContainerAncestors)))
                .Returns(searchContainerItemCollection);

            // act
            await _subjectUnderTest.GetSavedSearchesTreeAsync(workspaceArtifactId, null, savedSearchId).ConfigureAwait(false);

            // assert
            await _searchContainerManager.Received().GetSearchContainerTreeAsync(workspaceArtifactId, Arg.Is<List<int>>(x => x.SequenceEqual(savedSearchContainerAncestors))).ConfigureAwait(false);
        }

        [Test]
        public async Task GetTreeWithVisibleSavedSearch_ItShouldQueryTreeCreatorWithProperParameters()
        {
            int savedSearchId = 1;
            int[] savedSearchContainerAncestors = { 2, 3 };
            InitializeSavedSearchAncestors(savedSearchId, savedSearchContainerAncestors);

            var searchContainerItemCollection = new SearchContainerItemCollection();
            searchContainerItemCollection.SearchContainerItems = new List<SearchContainerItem>
            {
                new SearchContainerItem {SearchContainer = new SearchContainerRef(97)}
            };
            searchContainerItemCollection.SavedSearchContainerItems = new List<SavedSearchContainerItem>
            {
                new SavedSearchContainerItem {Secured = false, SavedSearch = new SavedSearchRef(45)},
                new SavedSearchContainerItem {Secured = true, SavedSearch = new SavedSearchRef(46)},
                new SavedSearchContainerItem {Secured = false, SavedSearch = new SavedSearchRef(47)}
            };
            _searchContainerManager.GetSearchContainerTreeAsync(workspaceArtifactId, Arg.Any<List<int>>())
                .Returns(searchContainerItemCollection);

            // act
            await _subjectUnderTest.GetSavedSearchesTreeAsync(workspaceArtifactId, null, savedSearchId).ConfigureAwait(false);

            // assert
            Expression<Predicate<IEnumerable<SearchContainerItem>>> areSearchContainersValid = containers => containers.Count() == 1;
            Expression<Predicate<IEnumerable<SavedSearchContainerItem>>> areSearchContainersItemValid = savedSearchItems =>
            savedSearchItems.Select(item => item.SavedSearch.ArtifactID).SequenceEqual(new[] { 45, 47 });

            _treeCreator.Received().Create(Arg.Is(areSearchContainersValid), Arg.Is(areSearchContainersItemValid));
        }

        [Test]
        public async Task GetTreeForDirectChildrenOfNode_ItShouldQueryForChildrenWithProperParameters()
        {
            int nodeId = 5;

            SearchContainerItemCollection children = new SearchContainerItemCollection();

            _searchContainerManager.GetSearchContainerItemsAsync(workspaceArtifactId, Arg.Any<SearchContainerRef>())
                .Returns(children);

            // act
            await _subjectUnderTest.GetSavedSearchesTreeAsync(workspaceArtifactId, nodeId).ConfigureAwait(false);

            // assert
            await _searchContainerManager.Received()
                .GetSearchContainerItemsAsync(workspaceArtifactId, Arg.Is<SearchContainerRef>(x => x.ArtifactID == nodeId)).ConfigureAwait(false);
        }

        [Test]
        public async Task GetTreeForDirectChildrenOfNode_ItShouldQueryForRootWithProperParameters()
        {
            int nodeId = 5;

            SearchContainerItemCollection children = new SearchContainerItemCollection();

            _searchContainerManager.GetSearchContainerItemsAsync(workspaceArtifactId, Arg.Any<SearchContainerRef>())
            .Returns(children);

            // act
            await _subjectUnderTest.GetSavedSearchesTreeAsync(workspaceArtifactId, nodeId).ConfigureAwait(false);

            // assert
            await _searchContainerManager.Received()
                .ReadSingleAsync(workspaceArtifactId, nodeId).ConfigureAwait(false);

        }

        [Test]
        public async Task GetTreeForDirectChildrenOfNode_ItShouldQueryTreeCreatorWithProperParameters()
        {
            int nodeId = 5;

            var parentContainer = new SearchContainer
            {
                ArtifactID = nodeId
            };
            _searchContainerManager.ReadSingleAsync(workspaceArtifactId, nodeId).Returns(parentContainer);

            var children = new SearchContainerItemCollection();
            children.SearchContainerItems = new List<SearchContainerItem>
            {
                new SearchContainerItem {SearchContainer = new SearchContainerRef(97)}
            };
            children.SavedSearchContainerItems = new List<SavedSearchContainerItem>
            {
                new SavedSearchContainerItem {Secured = false, SavedSearch = new SavedSearchRef(45)},
                new SavedSearchContainerItem {Secured = true, SavedSearch = new SavedSearchRef(46)},
                new SavedSearchContainerItem {Secured = false, SavedSearch = new SavedSearchRef(47)}
            };

            _searchContainerManager.GetSearchContainerItemsAsync(workspaceArtifactId, Arg.Any<SearchContainerRef>())
                .Returns(children);

            // act
            await _subjectUnderTest.GetSavedSearchesTreeAsync(workspaceArtifactId, nodeId).ConfigureAwait(false);

            // assert
            Expression<Predicate<IEnumerable<SearchContainerItem>>> areSearchContainersValid = containers => containers.Count() == 1;
            Expression<Predicate<IEnumerable<SavedSearchContainerItem>>> areSearchContainersItemValid = savedSearchItems =>
                savedSearchItems.Select(item => item.SavedSearch.ArtifactID).SequenceEqual(new[] { 45, 47 });

            _treeCreator.Received().CreateTreeForNodeAndDirectChildren(Arg.Is<SearchContainer>(x=>x.ArtifactID == nodeId), Arg.Is(areSearchContainersValid), Arg.Is(areSearchContainersItemValid));

        }

        private void InitializeSavedSearchAncestors(int savedSearchId, int[] ancestorsIds)
        {
            var savedSearch = new SavedSearchDTO
            {
                ArtifactId = savedSearchId,
                ParentContainerId = ancestorsIds[0]
            };
            _savedSearchQueryRepository.RetrieveSavedSearch(savedSearchId).Returns(savedSearch);

            for (int i = 0; i < ancestorsIds.Length; i++)
            {
                if (i == ancestorsIds.Length - 1) // i is last element
                {
                    _searchContainerManager.ReadSingleAsync(workspaceArtifactId, ancestorsIds[i]).Throws(new Exception());
                }
                else
                {
                    var searchContainer = new SearchContainer
                    {
                        ArtifactID = ancestorsIds[i],
                        ParentSearchContainer = new SearchContainerRef(ancestorsIds[i + 1])
                    };

                    _searchContainerManager.ReadSingleAsync(workspaceArtifactId, ancestorsIds[i]).Returns(searchContainer);
                }
            }

        }
    }
}

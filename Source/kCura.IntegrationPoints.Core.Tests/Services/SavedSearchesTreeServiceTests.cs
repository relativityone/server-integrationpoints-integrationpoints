using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Tests.Helpers;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.Search;

namespace kCura.IntegrationPoints.Core.Tests.Services
{
    [TestFixture]
    public class SavedSearchesTreeServiceTests
    {
        [Test]
        public void ItShouldReturnSavedSearchesTree()
        {
            // arrange
            var resultSet = new SearchContainerQueryResultSet();
            resultSet.Results.Add(new Result<SearchContainer>() { Artifact = new SearchContainer { ArtifactID = 1001 } });

            var searchContainerManager = Substitute.For<ISearchContainerManager>();
            searchContainerManager.QueryAsync(Arg.Any<int>(), Arg.Any<Query>())
                .Returns(resultSet);
            searchContainerManager.GetSearchContainerTreeAsync(Arg.Any<int>(), Arg.Any<List<int>>())
                .Returns(SavedSearchesTreeTestHelper.GetSampleContainerCollection());

            var servicesManager = Substitute.For<IServicesMgr>();
            servicesManager.CreateProxy<ISearchContainerManager>(Arg.Any<ExecutionIdentity>())
                .Returns(searchContainerManager);

            var helper = Substitute.For<IHelper>();
            helper.GetServicesManager()
                .Returns(servicesManager);

            var expected = SavedSearchesTreeTestHelper.GetSampleTreeWithSearches();

            var creator = Substitute.For<ISavedSearchesTreeCreator>();
            creator.Create(Arg.Any<IEnumerable<SearchContainerItem>>(), Arg.Any<IEnumerable<SavedSearchContainerItem>>())
                .Returns(expected);

            var service = new SavedSearchesTreeService(helper, creator);

            var workspaceArtifactId = 1042;

            // act
            var actual = service.GetSavedSearchesTree(workspaceArtifactId);

            // assert
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.Id, Is.EqualTo(expected.Id));
            Assert.That(actual.Text, Is.EqualTo(expected.Text));
            Assert.That(actual.Children.Count, Is.EqualTo(expected.Children.Count));
        }
    }
}
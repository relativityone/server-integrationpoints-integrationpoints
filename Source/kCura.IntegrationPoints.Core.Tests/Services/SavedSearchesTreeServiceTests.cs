using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Castle.Components.DictionaryAdapter;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Tests.Helpers;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.Search;

namespace kCura.IntegrationPoints.Core.Tests.Services
{
	[TestFixture]
	public class SavedSearchesTreeServiceTests : TestBase
	{
		[SetUp]
		public override void SetUp()
		{

		}

		[Test]
		public void ItShouldReturnSavedSearchesTree()
		{
			// arrange
			int workspaceArtifactId = 1042;

			var repoFactoryMock = Substitute.For<IRepositoryFactory>();
			var savedSearchQueryRepoMock = Substitute.For<ISavedSearchQueryRepository>();
		    var searchContainerManager = Substitute.For<ISearchContainerManager>();
            var servicesManager = Substitute.For<IServicesMgr>();
            var helper = Substitute.For<IHelper>();
		    var creator = Substitute.For<ISavedSearchesTreeCreator>();

		    servicesManager.CreateProxy<ISearchContainerManager>(Arg.Any<ExecutionIdentity>())
		        .Returns(searchContainerManager);
		    helper.GetServicesManager()
		        .Returns(servicesManager);
		    repoFactoryMock.GetSavedSearchQueryRepository(workspaceArtifactId).Returns(savedSearchQueryRepoMock);

            var subjectUnderTest = new SavedSearchesTreeService(helper, creator, repoFactoryMock);


			

            var folders = new SearchContainerQueryResultSet();
            folders.Results = new EditableList<Result<SearchContainer>>();
		    foreach (var folder in SavedSearchesTreeTestHelper.GetSampleContainerCollection().SearchContainerItems)
		    {
		        var result = new Result<SearchContainer>() { Success = true};
                result.Artifact = new SearchContainer();
		        result.Artifact.ArtifactID = folder.SearchContainer.ArtifactID;

                folders.Results.Add( result );
		    }


            searchContainerManager.QueryAsync(workspaceArtifactId, Arg.Any<Query>()).Returns( folders );
			searchContainerManager.GetSearchContainerTreeAsync(Arg.Any<int>(), Arg.Any<List<int>>())
				.Returns(SavedSearchesTreeTestHelper.GetSampleContainerCollection());

			var expected = SavedSearchesTreeTestHelper.GetSampleTreeWithSearches();
			
			creator.Create(Arg.Any<IEnumerable<SearchContainerItem>>(), Arg.Any<IEnumerable<SavedSearchContainerItem>>()).Returns(expected);

			// act
			var actual = subjectUnderTest.GetSavedSearchesTree(workspaceArtifactId);

			// assert
			Assert.That(actual, Is.Not.Null);
			Assert.That(actual.Id, Is.EqualTo(expected.Id));
			Assert.That(actual.Text, Is.EqualTo(expected.Text));
			Assert.That(actual.Children.Count, Is.EqualTo(expected.Children.Count));
		}
	}
}
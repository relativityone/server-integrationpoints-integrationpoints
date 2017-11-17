﻿using System.Collections;
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
using Relativity.Core.Service;
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

	    private SearchContainerQueryResultSet GetAllFolders()
	    {
	        var searchContainerQueryResultSet = new SearchContainerQueryResultSet();
	        searchContainerQueryResultSet.Results = new EditableList<Result<SearchContainer>>();
	        foreach (var searchContainerItem in SavedSearchesTreeTestHelper.GetSampleContainerCollection().SearchContainerItems)
	        {
	            var item = new Result<SearchContainer>() { Success = true, Artifact = new SearchContainer() { ArtifactID = searchContainerItem.SearchContainer.ArtifactID } };
	            searchContainerQueryResultSet.Results.Add(item);
	        }
	        return searchContainerQueryResultSet;
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
		    var htmlSanitizer = Substitute.For<IHtmlSanitizerManager>();

		    servicesManager.CreateProxy<ISearchContainerManager>(Arg.Any<ExecutionIdentity>())
		        .Returns(searchContainerManager);
		    helper.GetServicesManager()
		        .Returns(servicesManager);
		    repoFactoryMock.GetSavedSearchQueryRepository(workspaceArtifactId).Returns(savedSearchQueryRepoMock);

            var subjectUnderTest = new SavedSearchesTreeService(helper, creator, repoFactoryMock, htmlSanitizer);

            searchContainerManager.QueryAsync(workspaceArtifactId, Arg.Any<Query>()).Returns( GetAllFolders() );
			searchContainerManager.GetSearchContainerTreeAsync(workspaceArtifactId, 
                Arg.Is<List<int>>( list => list.SequenceEqual( SavedSearchesTreeTestHelper.GetSampleContainerIds() )))
				.Returns(SavedSearchesTreeTestHelper.GetSampleContainerCollection());

		    creator.Create(
		            Arg.Any<IEnumerable<SearchContainerItem>>(),
		            Arg.Any<IEnumerable<SavedSearchContainerItem>>())
		        .Returns(SavedSearchesTreeTestHelper.GetSampleTreeWithSearches());

			// act
			var actual = subjectUnderTest.GetSavedSearchesTree(workspaceArtifactId);

			// assert
			Assert.That(actual, Is.Not.Null);

		    var expected = SavedSearchesTreeTestHelper.GetSampleTreeWithSearches();
            Assert.That(actual.Id, Is.EqualTo(expected.Id));
			Assert.That(actual.Text, Is.EqualTo(expected.Text));
			Assert.That(actual.Children.Count, Is.EqualTo(expected.Children.Count));

            //check mock arguments
		    creator.Received(1).Create(
		            Arg.Is<IEnumerable<SearchContainerItem>>( list => list.Count() == SavedSearchesTreeTestHelper.GetSampleContainerCollection().SearchContainerItems.Count ),
		            Arg.Is<IEnumerable<SavedSearchContainerItem>>( list => list.Count() != SavedSearchesTreeTestHelper.GetSampleContainerCollection().SavedSearchContainerItems.Count));

		    var publicSearches = SavedSearchesTreeTestHelper.GetSampleContainerCollection().SavedSearchContainerItems
		        .Where(s => !s.Secured);

		    creator.Received(1).Create(
		        Arg.Is<IEnumerable<SearchContainerItem>>(list => list.Count() == SavedSearchesTreeTestHelper.GetSampleContainerCollection().SearchContainerItems.Count),
		        Arg.Is<IEnumerable<SavedSearchContainerItem>>(list => list.Count() == publicSearches.Count()));
        }

	    [Test]
	    public void ItShouldSanitizeNodesTextInTree()
	    {
	        // arrange
	        IHelper helper = Substitute.For<IHelper>();
	        ISavedSearchesTreeCreator creator = Substitute.For<ISavedSearchesTreeCreator>();
	        IRepositoryFactory repoFactoryMock = Substitute.For<IRepositoryFactory>();
	        IHtmlSanitizerManager htmlSanitizer = Substitute.For<IHtmlSanitizerManager>();

	        JsTreeItemDTO badTree = SavedSearchesTreeTestHelper.GetSampleTreeWithSearchesBeforeSanitize();
	        JsTreeItemDTO goodTree = SavedSearchesTreeTestHelper.GetSampleTreeWithSearchesAfterSanitize();
	        var mockedTextPairs = SavedSearchesTreeTestHelper.GetNodesNames(badTree)
	            .Zip(SavedSearchesTreeTestHelper.GetNodesNames(goodTree), (s1, s2) => new {S1 = s1, S2 = s2});
	        foreach (var pair in mockedTextPairs)
	        {
	            htmlSanitizer.Sanitize(pair.S1).Returns(new SanitizeResult() { CleanHTML = pair.S2, HasErrors = false });
	        }

	        var counter = 0;
            htmlSanitizer.When(x => x.Sanitize(Arg.Any<string>())).Do(_ => counter++);
	        var subjectUnderTest = new SavedSearchesTreeService(helper, creator, repoFactoryMock, htmlSanitizer);

	        // act 
	        JsTreeItemDTO sanitizedTree = subjectUnderTest.SanitizeTree(badTree);

	        // assert 
	        Assert.AreEqual(mockedTextPairs.Count(), counter, "Sanitize method was called different number of times than expected!");
	    }

	}
}
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;
using Relativity.Core.Service;
using Relativity.Services.Search;

namespace kCura.IntegrationPoints.Core.Tests.Helpers
{
	[TestFixture]
	public class SavedSearchesTreeCreatorTests : TestBase
	{
		[SetUp]
		public override void SetUp()
		{

		}

		private void SetupHtmlSanitizerMock(IHtmlSanitizerManager htmlSanitizer)
		{
			htmlSanitizer.Sanitize("<p>Kierkegaard</p>Platon").Returns(new SanitizeResult() { CleanHTML = "Platon", HasErrors = false });
			htmlSanitizer.Sanitize("<img src=x onerror=alert(Saved Search 1) />Nitsche").Returns(new SanitizeResult() { CleanHTML = "Nitsche", HasErrors = false });
			htmlSanitizer.Sanitize("<em>No-one survive</em>").Returns(new SanitizeResult() { CleanHTML = "Sanitized Search Name", HasErrors = false });
			htmlSanitizer.Sanitize("Search Folder 3").Returns(new SanitizeResult() { CleanHTML = "Search Folder 3", HasErrors = false });
			htmlSanitizer.Sanitize("<i>Saved </i>Search 1").Returns(new SanitizeResult() { CleanHTML = "Search 1", HasErrors = false });
			htmlSanitizer.Sanitize("<king>Search </king>2").Returns(new SanitizeResult() { CleanHTML = "2", HasErrors = false });
			htmlSanitizer.Sanitize("<em>Saved Search 3</em>").Returns(new SanitizeResult() { CleanHTML = "Sanitized Search Name", HasErrors = false });
		}

		[Test]
		public void ItShouldCreateTree()
		{
			// arrange
			var htmlSanitizer = Substitute.For<IHtmlSanitizerManager>();
			SetupHtmlSanitizerMock(htmlSanitizer);
			var counter = 0;
			htmlSanitizer.When(x => x.Sanitize(Arg.Any<string>())).Do(_ => counter++);
			var creator = new SavedSearchesTreeCreator(htmlSanitizer);
			var collection = SavedSearchesTreeTestHelper.GetSampleToSanitizeContainerCollection();
			var expected = SavedSearchesTreeTestHelper.GetSampleTree();

			// act
			var actual = creator.Create(collection.SearchContainerItems);

			// assert
			Assert.That(actual, Is.Not.Null);
			// root 
			Assert.That(actual.Id, Is.EqualTo(expected.Id));
			Assert.That(actual.Text, Is.EqualTo(expected.Text));
			Assert.That(actual.Children.Count, Is.EqualTo(expected.Children.Count));
			// first level folders
			Assert.That(actual.Children[0].Id, Is.EqualTo(expected.Children[0].Id));
			Assert.That(actual.Children[0].Text, Is.EqualTo(expected.Children[0].Text));
			Assert.That(actual.Children[0].Children.Count, Is.EqualTo(expected.Children[0].Children.Count));
			// second level folders
			Assert.That(actual.Children[0].Children[0].Id, Is.EqualTo(expected.Children[0].Children[0].Id));
			Assert.That(actual.Children[0].Children[0].Text, Is.EqualTo(expected.Children[0].Children[0].Text));
			Assert.That(actual.Children[0].Children[0].Children.Count, Is.EqualTo(expected.Children[0].Children[0].Children.Count));
			Assert.AreEqual(collection.SearchContainerItems.Count, counter, "Sanitize method was called different number of times than expected!");
		}

		[Test]
		public void ItShouldCreateTreeWithSearches()
		{
			// arrange
			var htmlSanitizer = Substitute.For<IHtmlSanitizerManager>();
			SetupHtmlSanitizerMock(htmlSanitizer);
			var counter = 0;
			htmlSanitizer.When(x => x.Sanitize(Arg.Any<string>())).Do(_ => counter++);
			var creator = new SavedSearchesTreeCreator(htmlSanitizer);
			var collection = SavedSearchesTreeTestHelper.GetSampleToSanitizeContainerCollection();
			JsTreeItemDTO expected = SavedSearchesTreeTestHelper.GetSampleTreeWithSearches();

			// act
			var actual = creator.Create(collection.SearchContainerItems, collection.SavedSearchContainerItems);

			// assert
			Assert.That(actual, Is.Not.Null);
			JsTreeItemDTO actualSearch1 = actual.Children.FirstOrDefault(x => x.Text.Contains("Sanitized Search Name"));
			JsTreeItemDTO expectedSearch1 = expected.Children.First(x => x.Text.Contains("Sanitized Search Name"));
			Assert.That(actualSearch1, Is.Not.Null);
			Assert.That(actualSearch1.Id, Is.EqualTo(expectedSearch1.Id));
			Assert.That(actualSearch1.Text, Is.EqualTo(expectedSearch1.Text));
			Assert.AreEqual(collection.SearchContainerItems.Count + collection.SavedSearchContainerItems.Count, counter, "Sanitize method was called different number of times than expected!");
		}

		[TestCase(0, 0)]
		[TestCase(1, 0)]
		[TestCase(0, 1)]
		[TestCase(3, 3)]
		[TestCase(10, 4)]
		[TestCase(10, 40)]
		public void ItShouldCreateTreeForNodeAndItsChildren(int numberOfChildContainer, int numberOfChildSearches)
		{
			// arrange
			string sanitizedText = "Sanitized";
			int parentArtifactId = 64532;
			string parentName = "Parent container";
			int childContainerStartArtifactId = 100000;
			int childSearchStartArtifactId = 200000;

			var htmlSanitizer = Substitute.For<IHtmlSanitizerManager>();
			htmlSanitizer.Sanitize(Arg.Any<string>()).Returns(new SanitizeResult { CleanHTML = sanitizedText, HasErrors = false });

			List<SearchContainerItem> childContainers = Enumerable
				.Range(childContainerStartArtifactId, numberOfChildContainer)
				.Select(CreateChildContainer).ToList();

			List<SavedSearchContainerItem> childSearches = Enumerable
				.Range(childSearchStartArtifactId, numberOfChildSearches)
				.Select(CreateChildSearch).ToList();

			var creator = new SavedSearchesTreeCreator(htmlSanitizer);
			var parent = new SearchContainer
			{
				ArtifactID = parentArtifactId,
				Name = parentName
			};

			// act
			JsTreeItemDTO actualResult = creator.CreateTreeForNodeAndDirectChildren(parent, childContainers, childSearches);

			// assert
			Assert.IsNotNull(actualResult);

			// validate sanitizer was used to each name
			IEnumerable<string> allNames = childContainers.Select(x => x.SearchContainer.Name)
				.Concat(childSearches.Select(y => y.SavedSearch.Name))
				.Concat(new[] { parentName });
			foreach (string name in allNames)
			{
				htmlSanitizer.Received().Sanitize(name);
			}

			// validate parent
			Assert.AreEqual(parent.ArtifactID.ToString(), actualResult.Id);
			Assert.AreEqual(true, actualResult.IsDirectory);
			Assert.AreEqual(sanitizedText, actualResult.Text);

			// validate children
			Assert.AreEqual(numberOfChildContainer + numberOfChildSearches, actualResult.Children.Count);

			// validate child containers
			foreach (SearchContainerItem childContainer in childContainers)
			{
				JsTreeItemDTO treeItemForContainer =
					actualResult.Children.Single(x => x.Id == childContainer.SearchContainer.ArtifactID.ToString());
				Assert.IsNotNull(treeItemForContainer);
				Assert.IsTrue(treeItemForContainer.IsDirectory);
				Assert.AreEqual(sanitizedText, treeItemForContainer.Text);
			}

			// validate child searches
			foreach (SavedSearchContainerItem childSearch in childSearches)
			{
				JsTreeItemDTO treeItemForSearch =
					actualResult.Children.Single(x => x.Id == childSearch.SavedSearch.ArtifactID.ToString());
				Assert.IsNotNull(treeItemForSearch);
				Assert.IsFalse(treeItemForSearch.IsDirectory);
				Assert.AreEqual(sanitizedText, treeItemForSearch.Text);
			}
		}

		private SearchContainerItem CreateChildContainer(int artifactId)
		{
			return new SearchContainerItem
			{
				SearchContainer = new SearchContainerRef(artifactId) { Name = $"Container {artifactId}" },
				HasChildren = true
			};
		}

		private SavedSearchContainerItem CreateChildSearch(int artifactId)
		{
			return new SavedSearchContainerItem
			{
				SavedSearch = new SavedSearchRef(artifactId)
				{
					Name = $"Search {artifactId}"
				},
				Personal = false
			};
		}
	}
}
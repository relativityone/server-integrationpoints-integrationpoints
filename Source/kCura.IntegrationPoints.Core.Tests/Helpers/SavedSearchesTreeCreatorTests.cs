using System.Collections.Generic;
using System.Linq;
using kCura.HTMLSanitizer;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;
using Relativity.Core.Service;

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

	}
}
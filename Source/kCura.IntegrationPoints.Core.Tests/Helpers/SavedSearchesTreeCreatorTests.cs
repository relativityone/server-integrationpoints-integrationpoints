using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Helpers
{
	[TestFixture]
	public class SavedSearchesTreeCreatorTests : TestBase
	{
		[SetUp]
		public override void SetUp()
		{
			
		}

		[Test]
		public void ItShouldCreateTree()
		{
			// arrange
			var creator = new SavedSearchesTreeCreator();
			var collection = SavedSearchesTreeTestHelper.GetSampleContainerCollection();
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
		}

		[Test]
		public void ItShouldCreateTreeWithSearches()
		{
			// arrange
			var creator = new SavedSearchesTreeCreator();
			var collection = SavedSearchesTreeTestHelper.GetSampleContainerCollection();
			var expected = SavedSearchesTreeTestHelper.GetSampleTreeWithSearches();

			// act
			var actual = creator.Create(collection.SearchContainerItems, collection.SavedSearchContainerItems);

			// assert
			Assert.That(actual, Is.Not.Null);
			var actualSearch1 = actual.Children.Where(x => x.Text.Contains("Saved Search")).FirstOrDefault();
			var expectedSearch1 = expected.Children.Where(x => x.Text.Contains("Saved Search")).First();
			Assert.That(actualSearch1, Is.Not.Null);
			Assert.That(actualSearch1.Id, Is.EqualTo(expectedSearch1.Id));
			Assert.That(actualSearch1.Text, Is.EqualTo(expectedSearch1.Text));
		}
	}
}
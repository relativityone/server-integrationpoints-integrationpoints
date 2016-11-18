using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Web.Models;
using kCura.Relativity.Client;
using NSubstitute;
using NUnit.Framework;
using Relativity.Core.Service;

namespace kCura.IntegrationPoints.Web.Tests.Controllers
{
	[TestFixture]
	public class SavedSearchQueryTests : TestBase
	{
		private IHtmlSanitizerManager _htmlSanitizerManager;

		[SetUp]
		public override void SetUp()
		{
			_htmlSanitizerManager = Substitute.For<IHtmlSanitizerManager>();
		}

		[Test]
		public void SavedSearchQueryFailsToRetrieveData()
		{
			// arrange
			const string errorMessage = "#failed";
			QueryResult result = new QueryResult() { Success = false, Message = errorMessage };
			IRSAPIClient rsapiClient = Substitute.For<IRSAPIClient>();
			rsapiClient.Query(Arg.Any<APIOptions>(), Arg.Any<Query>()).Returns(result);

			// act & assert
			Assert.Throws<Exception>(() => SavedSearchModel.GetAllPublicSavedSearches(rsapiClient, _htmlSanitizerManager), errorMessage);
		}


		[Test]
		public void SavedSearchQueryReturnsNoSavedSearch()
		{
			// arrange
			QueryResult result = new QueryResult() { Success = true};
			IRSAPIClient rsapiClient = Substitute.For<IRSAPIClient>();
			rsapiClient.Query(Arg.Any<APIOptions>(), Arg.Any<Query>()).Returns(result);

			// act
			List<SavedSearchModel> searches = SavedSearchModel.GetAllPublicSavedSearches(rsapiClient, _htmlSanitizerManager).ToList();

			// assert
			Assert.NotNull(searches);
			Assert.IsEmpty(searches);
		}

		[Test]
		public void SavedSearchQueryReturnsOneSavedSearch()
		{
			// arrange
			string searchName = "secrete search";
			_htmlSanitizerManager.Sanitize(searchName).Returns(new SanitizeResult() { HasErrors = false, CleanHTML = searchName });
			byte[] searchNameInBytes = Encoding.Unicode.GetBytes(searchName);
			Artifact savedSearch = new	Artifact();
			savedSearch.Fields = new List<Field>();
			savedSearch.Fields.Add(new Field() { Name = "Owner" , Value = null });
			savedSearch.Fields.Add(new Field() { Name = "Text Identifier", Value = searchNameInBytes });

			QueryResult result = new QueryResult() { Success = true };
			result.QueryArtifacts.Add(savedSearch);

			IRSAPIClient rsapiClient = Substitute.For<IRSAPIClient>();
			rsapiClient.Query(Arg.Any<APIOptions>(), Arg.Any<Query>()).Returns(result);

			// act
			IList<SavedSearchModel> searches = SavedSearchModel.GetAllPublicSavedSearches(rsapiClient, _htmlSanitizerManager);

			// assert
			Assert.NotNull(searches);
			Assert.AreEqual(1, searches.Count);
		}

		[Test]
		public void SavedSearchQueryReturnsOneSavedSearch_ButTheSearchHasTheOwner()
		{
			// arrange
			string searchName = "secrete search";
			_htmlSanitizerManager.Sanitize(searchName).Returns(new SanitizeResult() { HasErrors = false, CleanHTML = searchName });
			byte[] searchNameInBytes = Encoding.Unicode.GetBytes(searchName);

			Artifact savedSearch = new Artifact
			{
				Fields = new List<Field>
				{
					new Field() {Name = "Owner", Value = searchNameInBytes},
					new Field() {Name = "Text Identifier", Value = searchNameInBytes}
				}
			};

			QueryResult result = new QueryResult() { Success = true };
			result.QueryArtifacts.Add(savedSearch);

			IRSAPIClient rsapiClient = Substitute.For<IRSAPIClient>();
			rsapiClient.Query(Arg.Any<APIOptions>(), Arg.Any<Query>()).Returns(result);

			// act
			List<SavedSearchModel> searches = SavedSearchModel.GetAllPublicSavedSearches(rsapiClient, _htmlSanitizerManager).ToList();

			// assert
			Assert.NotNull(searches);
			Assert.IsEmpty(searches);
		}
	}
}
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Web.Controllers.API;
using NUnit.Framework;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.Web.Tests.Integration.Controllers
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	[NotWorkingOnTrident]
	public class SavedSearchQueryTests : SourceProviderTemplate
	{
		private List<int> _savedSearchesArtifactIds;
		private List<int> _userIds;
		private List<int> _groupIds;

		public SavedSearchQueryTests() : base("SavedSearchQueryTests")
		{
		}

		public async override void SuiteSetup()
		{
			base.SuiteSetup();
			await InstanceSetting.CreateOrUpdateAsync("Relativity.Authentication", "AdminsCanSetPasswords", "True")
				.ConfigureAwait(false);
		}

		public override void TestSetup()
		{
			_groupIds = new List<int>();
			_userIds = new List<int>();
			_savedSearchesArtifactIds = new List<int>();
		}

		public override void TestTeardown()
		{
			foreach (var artifactId in _savedSearchesArtifactIds)
			{
				SavedSearch.Delete(WorkspaceArtifactId, artifactId);
			}
			foreach (var artifactId in _userIds)
			{
				User.DeleteUser(artifactId);
			}
			foreach (var artifactId in _groupIds)
			{
				Group.DeleteGroup(artifactId);
			}
		}

		[IdentifiedTest("141453e2-6363-4e1d-b476-268a73027fa5")]
		[SmokeTest]
		public void Query_SavedSearchesWithController_Success()
		{
			//Arrange
			const string savedSearchName = "Public Saved Search";
			HttpResponseMessage httpResponseMessage;
			int savedSearchArtifactId = SavedSearch.CreateSavedSearch(WorkspaceArtifactId, savedSearchName);

			var repoFactory = new RepositoryFactory(Helper, Helper.GetServicesManager());
			//Act
			SavedSearchFinderController savedSearchFinderController = new SavedSearchFinderController(repoFactory) { Request = new HttpRequestMessage() };
			savedSearchFinderController.Request.SetConfiguration(new HttpConfiguration());
			httpResponseMessage = savedSearchFinderController.Get(WorkspaceArtifactId);

			string content = httpResponseMessage.Content.ReadAsStringAsync().Result;

			//Assert
			Assert.AreEqual(HttpStatusCode.OK, httpResponseMessage.StatusCode);
			StringAssert.Contains(savedSearchArtifactId.ToString(), content);
		}
	}
}

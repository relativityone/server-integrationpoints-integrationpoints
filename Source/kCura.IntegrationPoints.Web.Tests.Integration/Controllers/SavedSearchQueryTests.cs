using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.IntegrationPoints.Web.Models;
using kCura.Relativity.Client;
using NSubstitute;
using NUnit.Framework;
using Relativity.Core.Service;
using Relativity.Services.Field;
using Relativity.Services.Group;
using Relativity.Services.Permission;
using Relativity.Services.Search;
using Relativity.Services.User;

namespace kCura.IntegrationPoints.Web.Tests.Integration.Controllers
{
	[TestFixture]
	public class SavedSearchQueryTests : SourceProviderTemplate
	{
		private const string _CONTROLNUMBER = "Control Number";
		private List<int> _savedSearchesArtifactIds;
		private List<int> _userIds;
		private List<int> _groupIds;
		private IHtmlSanitizerManager _htmlSanitizerManage;

		public SavedSearchQueryTests() : base("SavedSearchQueryTests")
		{
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			InstanceSetting.UpsertAndReturnOldValueIfExists("Relativity.Authentication", "AdminsCanSetPasswords", "True");
		}

		public override void TestSetup()
		{
			_groupIds = new List<int>();
			_userIds = new List<int>();
			_savedSearchesArtifactIds = new List<int>();
			_htmlSanitizerManage = NSubstitute.Substitute.For<IHtmlSanitizerManager>();
			_htmlSanitizerManage.Sanitize(Arg.Any<string>()).Returns(new SanitizeResult() { CleanHTML = "Bla", HasErrors = false });
		}

		public override void TestTeardown()
		{
			Helper.RelativityUserName = SharedVariables.RelativityUserName;
			foreach (var artifactId in _savedSearchesArtifactIds)
			{
				SavedSearch.Delete(WorkspaceArtifactId, artifactId);
			}
			foreach (var artifactId in _userIds)
			{
				IntegrationPoint.Tests.Core.User.DeleteUser(artifactId);
			}
			foreach (var artifactId in _groupIds)
			{
				Group.DeleteGroup(artifactId);
			}
		}

		[Test]
		[Category(IntegrationPoint.Tests.Core.Constants.SMOKE_TEST)]
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
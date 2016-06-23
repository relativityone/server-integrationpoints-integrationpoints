using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Web.Models;
using kCura.Relativity.Client;
using NUnit.Framework;
using Relativity.Services.Field;
using Relativity.Services.Group;
using Relativity.Services.Permission;
using Relativity.Services.Search;
using Relativity.Services.User;
using Permission = kCura.IntegrationPoint.Tests.Core.Permission;
using User = kCura.IntegrationPoint.Tests.Core.User;

namespace kCura.IntegrationPoints.Web.Tests.Integration
{
	[TestFixture]
	[Category("Integration Tests")]
	public class SavedSearchQueryTests
		: SingleWorkspaceTestTemplate
	{
		private List<int> _savedSearchesArtifactIds;
		private List<int> _userIds;
		private List<int> _groupIds;

		public SavedSearchQueryTests()
			: base("SavedSearchQueryTests")
		{
		}

		[SetUp]
		public void TestSetup()
		{
			_groupIds = new List<int>();
			_userIds = new List<int>();
			_savedSearchesArtifactIds = new List<int>();
		}

		[TearDown]
		public void TestTearDown()
		{
			Helper.RelativityUserName = SharedVariables.RelativityUserName;
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

		[Test]
		[TestCase(1)]
		[TestCase(1010)]
		public void GetAllPublicSavedSearches(int savedSearchToCreate)
		{
			for (int i = 0; i < savedSearchToCreate; i++)
			{
				int artifactId = SavedSearch.CreateSavedSearch(WorkspaceArtifactId, "GetAllPublicSavedSearches_LotsOfSS_" + i);
				_savedSearchesArtifactIds.Add(artifactId);
			}

			IList<SavedSearchModel> results = null;
			using (IRSAPIClient rsapiClient = Helper.CreateUserProxy<IRSAPIClient>())
			{
				rsapiClient.APIOptions.WorkspaceID = WorkspaceArtifactId;
				results = SavedSearchModel.GetAllPublicSavedSearches(rsapiClient);
			}
			Assert.AreEqual(savedSearchToCreate, results.Count);
		}

		[Test]
		public void DoNotIncludePrivateSavedSearch()
		{
			KeywordSearch search = new KeywordSearch()
			{
				Name = "KWUuuuuuuuu",
				ArtifactTypeID = (int)ArtifactType.Document,
				Owner = new UserRef() { ArtifactID = 9 },
				Fields = new List<FieldRef>(new FieldRef[] { new FieldRef("Control Number"), })
			};
			int savedSearchId = SavedSearch.Create(WorkspaceArtifactId, search);
			_savedSearchesArtifactIds.Add(savedSearchId);

			IList<SavedSearchModel> results = null;
			using (IRSAPIClient rsapiClient = Helper.CreateUserProxy<IRSAPIClient>())
			{
				rsapiClient.APIOptions.WorkspaceID = WorkspaceArtifactId;
				results = SavedSearchModel.GetAllPublicSavedSearches(rsapiClient);
			}
			Assert.AreEqual(0, results.Count);
		}

		[Test]
		public void PublicSavedSearchInPublicFolder()
		{
			SearchContainer folder = new SearchContainer()
			{
				Name = "Testing Folder",
			};
			int folderArtifactId = SavedSearch.CreateSearchFolder(WorkspaceArtifactId, folder);

			KeywordSearch search = new KeywordSearch()
			{
				Name = "KWUuuuuuuuu",
				ArtifactTypeID = (int)ArtifactType.Document,
				SearchContainer = new SearchContainerRef(folderArtifactId),
				Fields = new List<FieldRef>(new FieldRef[] { new FieldRef("Control Number"), })
			};
			int savedSearchId = SavedSearch.Create(WorkspaceArtifactId, search);
			_savedSearchesArtifactIds.Add(savedSearchId);

			IList<SavedSearchModel> results = null;
			using (IRSAPIClient rsapiClient = Helper.CreateUserProxy<IRSAPIClient>())
			{
				rsapiClient.APIOptions.WorkspaceID = WorkspaceArtifactId;
				results = SavedSearchModel.GetAllPublicSavedSearches(rsapiClient);
			}
			Assert.AreEqual(1, results.Count);
		}

		[Test]
		public void SecuredSavedSearchWillBeExcluded()
		{
			string userName = "gbadman@kcura.com";

			int groupId = Group.CreateGroup("krowten");
			_groupIds.Add(groupId);

			UserModel user = User.CreateUser("Gerron", "BadMan", userName, new[] { groupId });
			_userIds.Add(user.ArtifactId);

			Group.AddGroupToWorkspace(WorkspaceArtifactId, groupId);
			SearchContainer folder = new SearchContainer()
			{
				Name = "Testing Folder",
			};
			int folderArtifactId = SavedSearch.CreateSearchFolder(WorkspaceArtifactId, folder);
			Permission.AddRemoveItemGroups(WorkspaceArtifactId, folderArtifactId, new GroupSelector() { DisabledGroups = new List<GroupRef>(new[] { new GroupRef(groupId) }) });

			KeywordSearch search = new KeywordSearch()
			{
				Name = "KWUuuuuuuuu",
				ArtifactTypeID = (int)ArtifactType.Document,
				SearchContainer = new SearchContainerRef(folderArtifactId),
				Fields = new List<FieldRef>(new FieldRef[] { new FieldRef("Control Number"), })
			};
			int savedSearchId = SavedSearch.Create(WorkspaceArtifactId, search);
			_savedSearchesArtifactIds.Add(savedSearchId);

			Helper.RelativityUserName = userName;
			IList<SavedSearchModel> results = null;
			using (IRSAPIClient rsapiClient = Helper.CreateUserProxy<IRSAPIClient>())
			{
				rsapiClient.APIOptions.WorkspaceID = WorkspaceArtifactId;
				results = SavedSearchModel.GetAllPublicSavedSearches(rsapiClient);
			}
			Assert.AreEqual(0, results.Count);
		}
	}
}
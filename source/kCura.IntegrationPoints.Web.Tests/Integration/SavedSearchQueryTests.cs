using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Web.Models;
using kCura.Relativity.Client;
using NUnit.Framework;
using Relativity.API;
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
		public SavedSearchQueryTests()
			: base("SavedSearchQueryTests")
		{
		}

		[Test]
		[TestCase(1)]
		[TestCase(1010)]
		public void GetAllPublicSavedSearches(int savedSearchToCreate)
		{
			List<int> savedSearchArtifactIds = new List<int>(savedSearchToCreate);
			try
			{
				for (int i = 0; i < savedSearchToCreate; i++)
				{
					int artifactId = SavedSearch.CreateSavedSearch(WorkspaceArtifactId, "GetAllPublicSavedSearches_LotsOfSS_" + i);
					savedSearchArtifactIds.Add(artifactId);
				}

				IList<SavedSearchModel> results = null;
				using (IRSAPIClient rsapiClient = Helper.CreateUserProxy<IRSAPIClient>())
				{
					rsapiClient.APIOptions.WorkspaceID = WorkspaceArtifactId;
					results = SavedSearchModel.GetAllPublicSavedSearches(rsapiClient);
				}
				Assert.AreEqual(savedSearchToCreate, results.Count);
			}
			finally
			{
				foreach (var artifactId in savedSearchArtifactIds)
				{
					SavedSearch.Delete(WorkspaceArtifactId, artifactId);
				}
			}
		}

		[Test]
		public void DoNotIncludePrivateSavedSearch()
		{
			int savedSearch = 0;
			try
			{
				KeywordSearch search = new KeywordSearch()
				{
					Name = "KWUuuuuuuuu",
					ArtifactTypeID = (int) ArtifactType.Document,
					Owner = new UserRef() {ArtifactID = 9},
					Fields = new List<FieldRef>(new FieldRef[] { new FieldRef("Control Number"),  })
				};
				SavedSearch.Create(WorkspaceArtifactId, search);

				IList<SavedSearchModel> results = null;
				using (IRSAPIClient rsapiClient = Helper.CreateUserProxy<IRSAPIClient>())
				{
					rsapiClient.APIOptions.WorkspaceID = WorkspaceArtifactId;
					results = SavedSearchModel.GetAllPublicSavedSearches(rsapiClient);
				}
				Assert.AreEqual(0, results.Count);
			}
			finally
			{
				SavedSearch.Delete(WorkspaceArtifactId, savedSearch);
			}
		}

		[Test]
		public void PublicSavedSearchInPublicFolder()
		{
			int savedSearchId = 0;
			try
			{
				SearchContainer folder = new SearchContainer()
				{
					Name = "Testing Folder",
				};
				int folderArtifactId = SavedSearch.CreateSearchFolder(WorkspaceArtifactId, folder);
				
				KeywordSearch search = new KeywordSearch()
				{
					Name = "KWUuuuuuuuu",
					ArtifactTypeID = (int) ArtifactType.Document,
					SearchContainer = new SearchContainerRef(folderArtifactId),
					Fields = new List<FieldRef>(new FieldRef[] {new FieldRef("Control Number"),})
				};
				savedSearchId = SavedSearch.Create(WorkspaceArtifactId, search);

				IList<SavedSearchModel> results = null;
				using (IRSAPIClient rsapiClient = Helper.CreateUserProxy<IRSAPIClient>())
				{
					rsapiClient.APIOptions.WorkspaceID = WorkspaceArtifactId;
					results = SavedSearchModel.GetAllPublicSavedSearches(rsapiClient);
				}
				Assert.AreEqual(1, results.Count);
			}
			finally
			{
				SavedSearch.Delete(WorkspaceArtifactId, savedSearchId);
			}
		}

		[Test]
		public void SecuredSavedSearchWillBeExcluded()
		{
			int savedSearchId = 0;
			UserModel user = null;
			try
			{
				int groupId = Group.CreateGroup("krowten");
				user = User.CreateUser("Gerron", "BadMan", "gbadman@kcura.com", new[] { groupId });
				Group.AddGroupToWorkspace(WorkspaceArtifactId, groupId);

				SearchContainer folder = new SearchContainer()
				{
					Name = "Testing Folder",
				};
				int folderArtifactId = SavedSearch.CreateSearchFolder(WorkspaceArtifactId, folder);
				Permission.AddRemoveItemGroups(WorkspaceArtifactId, folderArtifactId, new GroupSelector() { DisabledGroups = new List<GroupRef>(new [] { new GroupRef(groupId) })});

				KeywordSearch search = new KeywordSearch()
				{
					Name = "KWUuuuuuuuu",
					ArtifactTypeID = (int)ArtifactType.Document,
					SearchContainer = new SearchContainerRef(folderArtifactId),
					Fields = new List<FieldRef>(new FieldRef[] { new FieldRef("Control Number"), })
				};
				savedSearchId = SavedSearch.Create(WorkspaceArtifactId, search);

				IList<SavedSearchModel> results = null;
				using (IRSAPIClient rsapiClient = Helper.CreateUserProxy<IRSAPIClient>())
				{
					rsapiClient.APIOptions.WorkspaceID = WorkspaceArtifactId;
					results = SavedSearchModel.GetAllPublicSavedSearches(rsapiClient);
				}
				Assert.AreEqual(0, results.Count);
			}
			finally
			{
				if (user != null) User.DeleteUser(user.ArtifactId);
				SavedSearch.Delete(WorkspaceArtifactId, savedSearchId);
			}
		}
	}
}
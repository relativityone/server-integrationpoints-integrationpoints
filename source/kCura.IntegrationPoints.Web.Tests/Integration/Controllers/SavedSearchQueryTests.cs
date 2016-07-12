﻿using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Templates;
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

namespace kCura.IntegrationPoints.Web.Tests.Integration
{
	[TestFixture]
	[Category("Integration Tests")]
	public class SavedSearchQueryTests
		: SourceProviderTemplate
	{
		private const string _CONTROLNUMBER = "Control Number";
		private List<int> _savedSearchesArtifactIds;
		private List<int> _userIds;
		private List<int> _groupIds;
		private IHtmlSanitizerManager _htmlSanitizerManage;

		public SavedSearchQueryTests()
			: base("SavedSearchQueryTests")
		{
		}

		[TestFixtureSetUp]
		public new void SuiteSetup()
		{
			InstanceSetting.UpdateAndReturnOldValue("Relativity.Authentication", "AdminsCanSetPasswords", "True");
		}

		[SetUp]
		public void TestSetup()
		{
			_groupIds = new List<int>();
			_userIds = new List<int>();
			_savedSearchesArtifactIds = new List<int>();
			_htmlSanitizerManage = NSubstitute.Substitute.For<IHtmlSanitizerManager>();
			_htmlSanitizerManage.Sanitize(Arg.Any<string>()).Returns(new SanitizeResult() { CleanHTML = "Bla", HasErrors = false });
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
				IntegrationPoint.Tests.Core.User.DeleteUser(artifactId);
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
			// arrange
			for (int i = 0; i < savedSearchToCreate; i++)
			{
				int artifactId = SavedSearch.CreateSavedSearch(WorkspaceArtifactId, "GetAllPublicSavedSearches_LotsOfSS_" + i);
				_savedSearchesArtifactIds.Add(artifactId);
			}

			// act
			IList<SavedSearchModel> results = null;
			using (IRSAPIClient rsapiClient = Helper.CreateUserProxy<IRSAPIClient>())
			{
				rsapiClient.APIOptions.WorkspaceID = WorkspaceArtifactId;
				results = SavedSearchModel.GetAllPublicSavedSearches(rsapiClient, _htmlSanitizerManage);
			}

			// assert
			Assert.AreEqual(savedSearchToCreate, results.Count);
		}

		[Test]
		public void DoNotIncludePrivateSavedSearch()
		{
			// arrange
			KeywordSearch search = new KeywordSearch()
			{
				Name = "KWUuuuuuuuu",
				ArtifactTypeID = (int)ArtifactType.Document,
				Owner = new UserRef() { ArtifactID = 9 },
				Fields = new List<FieldRef>(new FieldRef[] { new FieldRef(_CONTROLNUMBER), })
			};
			int savedSearchId = SavedSearch.Create(WorkspaceArtifactId, search);
			_savedSearchesArtifactIds.Add(savedSearchId);

			// act
			IList<SavedSearchModel> results = null;
			using (IRSAPIClient rsapiClient = Helper.CreateUserProxy<IRSAPIClient>())
			{
				rsapiClient.APIOptions.WorkspaceID = WorkspaceArtifactId;
				results = SavedSearchModel.GetAllPublicSavedSearches(rsapiClient, _htmlSanitizerManage);
			}
			// assert
			Assert.AreEqual(0, results.Count);
		}

		[Test]
		public void PublicSavedSearchInPublicFolder()
		{
			// arrange
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
				Fields = new List<FieldRef>(new FieldRef[] { new FieldRef(_CONTROLNUMBER), })
			};
			int savedSearchId = SavedSearch.Create(WorkspaceArtifactId, search);
			_savedSearchesArtifactIds.Add(savedSearchId);

			// act
			IList<SavedSearchModel> results = null;
			using (IRSAPIClient rsapiClient = Helper.CreateUserProxy<IRSAPIClient>())
			{
				rsapiClient.APIOptions.WorkspaceID = WorkspaceArtifactId;
				results = SavedSearchModel.GetAllPublicSavedSearches(rsapiClient, _htmlSanitizerManage);
			}

			//assert
			Assert.AreEqual(1, results.Count);
		}

		[Test]
		public void SecuredSavedSearchWillBeExcluded()
		{
			// arrange
			string userName = "gbadman@kcura.com";

			int groupId = Group.CreateGroup("krowten");
			_groupIds.Add(groupId);

			UserModel user = IntegrationPoint.Tests.Core.User.CreateUser("Gerron", "BadMan", userName, new[] { groupId });
			_userIds.Add(user.ArtifactId);

			Group.AddGroupToWorkspace(WorkspaceArtifactId, groupId);
			SearchContainer folder = new SearchContainer()
			{
				Name = "Testing Folder",
			};
			int folderArtifactId = SavedSearch.CreateSearchFolder(WorkspaceArtifactId, folder);
			IntegrationPoint.Tests.Core.Permission.AddRemoveItemGroups(WorkspaceArtifactId, folderArtifactId, new GroupSelector() { DisabledGroups = new List<GroupRef>(new[] { new GroupRef(groupId) }) });

			KeywordSearch search = new KeywordSearch()
			{
				Name = "KWUuuuuuuuu",
				ArtifactTypeID = (int)ArtifactType.Document,
				SearchContainer = new SearchContainerRef(folderArtifactId),
				Fields = new List<FieldRef>(new FieldRef[] { new FieldRef(_CONTROLNUMBER), })
			};
			int savedSearchId = SavedSearch.Create(WorkspaceArtifactId, search);
			_savedSearchesArtifactIds.Add(savedSearchId);

			//act
			Helper.RelativityUserName = userName;
			IList<SavedSearchModel> results = null;
			using (IRSAPIClient rsapiClient = Helper.CreateUserProxy<IRSAPIClient>())
			{
				rsapiClient.APIOptions.WorkspaceID = WorkspaceArtifactId;
				results = SavedSearchModel.GetAllPublicSavedSearches(rsapiClient, _htmlSanitizerManage);
			}

			//assert
			Assert.AreEqual(0, results.Count);
		}
	}
}
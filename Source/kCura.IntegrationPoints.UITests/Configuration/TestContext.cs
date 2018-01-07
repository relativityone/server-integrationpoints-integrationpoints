using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using kCura.IntegrationPoints.UITests.Logging;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using NUnit.Framework;
using Relativity.Core;
using Serilog;

namespace kCura.IntegrationPoints.UITests.Configuration
{
	public class TestContext
	{

		private const int _ADMIN_USER_ID = 9;

		private const string _TEMPALTE_WKSP_NAME = "Relativity Starter Template";

		private readonly Lazy<ITestHelper> _helper;

		private static readonly ILogger Log = LoggerFactory.CreateLogger(typeof(TestContext));

		public readonly string TimeStamp;

		public ITestHelper Helper => _helper.Value;
		
		public int? WorkspaceId { get; private set; }

		public string WorkspaceName { get; set; }

		public int? GroupId { get; private set; }

		public int? UserId { get; private set; }

		public TestContext()
		{
			_helper = new Lazy<ITestHelper>(() => new TestHelper());
			TimeStamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
		}

		public TestContext CreateWorkspace()
		{
			WorkspaceName = $"Test Workspace {TimeStamp}";
			Log.Information($"Attempting to create workspace '{WorkspaceName}' using template '{_TEMPALTE_WKSP_NAME}'.");
			try
			{
				WorkspaceId = Workspace.CreateWorkspace(WorkspaceName, _TEMPALTE_WKSP_NAME);
			}
			catch (Exception ex)
			{
				Log.Error(ex, @"Cannot create workspace '{WorkspaceName}' using template '{_TEMPALTE_WKSP_NAME}'. Check if Relativity works correctly (services, ...).");
				throw;
			}
			Log.Information("Workspace '{WorkspaceName}' was successfully created using template '{_TEMPALTE_WKSP_NAME}.");
			return this;
		}

		public TestContext SetupUser()
		{
			GroupId = Group.CreateGroup($"TestGroup_{TimeStamp}");
			Group.AddGroupToWorkspace(GetWorkspaceId(), GetGroupId());

			ClaimsPrincipal.ClaimsPrincipalSelector += () =>
			{
				var factory = new ClaimsPrincipalFactory();
				return factory.CreateClaimsPrincipal2(_ADMIN_USER_ID, Helper);
			};

			UserModel userModel = User.CreateUser("UI", $"Test_User_{TimeStamp}", $"UI_Test_User_{TimeStamp}@relativity.com", new List<int> { GetGroupId() });
			UserId = userModel.ArtifactId;
			return this;
		}

		public TestContext InstallIntegrationPoints()
		{
			Assert.NotNull(WorkspaceId, $"{nameof(WorkspaceId)} is null. Workspace wasn't created.");

			Stopwatch stopwatch = Stopwatch.StartNew();
			try
			{
				ICoreContext coreContext = SourceProviderTemplate.GetBaseServiceContext(-1);

				var ipAppManager = new RelativityApplicationManager(coreContext, Helper);
				bool isAppInstalled = ipAppManager.IsGetApplicationInstalled((int) WorkspaceId);
				if (!isAppInstalled)
				{
					ipAppManager.InstallIntegrationPointFromAppLibraryToWorkspace((int) WorkspaceId);
				}
				Log.Information(@"Application is installed.");
			}
			catch (Exception ex)
			{
				Log.Error(ex, @"Unexpected error when detecting or installing Integration Points application in the workspace.");
				throw;
			}
			finally
			{
				Log.Information($"Installation of Integration Points in workspace took {stopwatch.Elapsed.Seconds} seconds.");
			}
			return this;
		}

		public TestContext ImportDocuments()
		{
			Log.Information(@"Importing documents...");
			string testDir = NUnit.Framework.TestContext.CurrentContext.TestDirectory.Replace("kCura.IntegrationPoints.UITests",
				"kCura.IntegrationPoint.Tests.Core");
			DocumentsTestData data = DocumentTestDataBuilder.BuildTestData(testDir);
			var workspaceService = new WorkspaceService(new ImportHelper());
			workspaceService.ImportData(GetWorkspaceId(), data);
			Log.Information(@"Documents imported.");
			return this;
		}

		public TestContext TearDown()
		{
			if (WorkspaceId.HasValue)
			{
				Workspace.DeleteWorkspace(GetWorkspaceId());
			}

			if (GroupId.HasValue)
			{
				Group.DeleteGroup(GetGroupId());
			}

			if (UserId.HasValue)
			{
				User.DeleteUser(GetUserId());
			}
			return this;
		}

		public int GetGroupId()
		{
			Assert.NotNull(GroupId, $"{nameof(GroupId)} is null. Group wasn't created.");
			return GroupId.Value;
		}

		public int GetWorkspaceId()
		{
			Assert.NotNull(WorkspaceId, $"{nameof(WorkspaceId)} is null. Workspace wasn't created.");
			return WorkspaceId.Value;
		}

		public int GetUserId()
		{
			Assert.NotNull(UserId, $"{nameof(UserId)} is null. User wasn't created.");
			return UserId.Value;
		}
	}

}

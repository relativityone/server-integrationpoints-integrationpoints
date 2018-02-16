using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using kCura.IntegrationPoints.UITests.Logging;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using NUnit.Framework;
using Relativity.Core;
using Serilog;
using Group = kCura.IntegrationPoint.Tests.Core.Group;
using User = kCura.IntegrationPoint.Tests.Core.User;
using Workspace = kCura.IntegrationPoint.Tests.Core.Workspace;

namespace kCura.IntegrationPoints.UITests.Configuration
{
	public class TestContext
	{

		private const int _ADMIN_USER_ID = 9;

		private const string _TEMPLATE_WKSP_NAME = "Smoke Workspace";

		private readonly Lazy<ITestHelper> _helper;

		private static readonly ILogger Log = LoggerFactory.CreateLogger(typeof(TestContext));

		public readonly string TimeStamp;

		public ITestHelper Helper => _helper.Value;
		
		public int? WorkspaceId { get; set; }

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
			CreateWorkspaceAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			return this;
		}

		public async Task CreateWorkspaceAsync()
		{
			WorkspaceName = $"Test Workspace {TimeStamp}";
			Log.Information($"Attempting to create workspace '{WorkspaceName}' using template '{_TEMPLATE_WKSP_NAME}'.");
			try
			{
				WorkspaceId = await Workspace.CreateWorkspaceAsync(WorkspaceName, _TEMPLATE_WKSP_NAME);
			}
			catch (Exception ex)
			{
				Log.Error(ex,
					@"Cannot create workspace '{WorkspaceName}' using template '{_TEMPLATE_WKSP_NAME}'. Check if Relativity works correctly (services, ...).");
				throw;
			}

			Log.Information("Workspace '{WorkspaceName}' was successfully created using template '{_TEMPLATE_WKSP_NAME}.");
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
				bool isAppInstalledAndUpToDate = ipAppManager.IsApplicationInstalledAndUpToDate((int) WorkspaceId);
				if (!isAppInstalledAndUpToDate)
				{
					ipAppManager.InstallApplicationFromLibrary((int) WorkspaceId);
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

		public async Task InstallIntegrationPointsAsync()
		{
			await Task.Run(() => InstallIntegrationPoints());
		}

		public TestContext ImportDocuments(bool withNatives = true, DocumentTestDataBuilder.TestDataType testDataType = DocumentTestDataBuilder.TestDataType.ModerateWithFoldersStructure)
		{
			Log.Information(@"Importing documents...");
			string testDir = NUnit.Framework.TestContext.CurrentContext.TestDirectory.Replace("kCura.IntegrationPoints.UITests",
				"kCura.IntegrationPoint.Tests.Core");
			DocumentsTestData data = DocumentTestDataBuilder.BuildTestData(testDir, withNatives, testDataType);
			var workspaceService = new WorkspaceService(new ImportHelper());
			workspaceService.ImportData(GetWorkspaceId(), data);
			Log.Information(@"Documents imported.");
			return this;
		}

		public async Task ImportDocumentsAsync(bool withNatives = true, DocumentTestDataBuilder.TestDataType testDataType = DocumentTestDataBuilder.TestDataType.ModerateWithFoldersStructure)
		{
			await Task.Run(() => ImportDocuments(withNatives, testDataType));
		}

		private RelativityScript FindRelativityFolderPathScript(IRSAPIClient proxy)
		{
			TextCondition nameCondition =
				new TextCondition(RelativityScriptFieldNames.Name, TextConditionEnum.EqualTo,
					"Set Relativity Folder Path Field");
			Query<RelativityScript> relScriptQuery = new Query<RelativityScript>()
			{
				Condition = nameCondition,
				Fields = FieldValue.NoFields
			};

			try
			{
				var relScriptQueryResults = proxy.Repositories.RelativityScript.Query(relScriptQuery);

				if (!relScriptQueryResults.Success)
				{
					Log.Error(@"An error occurred finding the script: {0}", relScriptQueryResults.Message);
				}
				return relScriptQueryResults.Results[0].Artifact;
			}
			catch (Exception ex)
			{
				Log.Error("An error occurred querying for Relativity Scripts: {0}", ex.Message);
			}
			return null;
		}

		public bool ExecuteRelativityFolderPathScript()
		{
			using (var client = Helper.CreateAdminProxy<IRSAPIClient>())
			{
				client.APIOptions.WorkspaceID = WorkspaceId.Value;

				var relativityScript = FindRelativityFolderPathScript(client);

				if (relativityScript != null)
				{
					var inputParameter = new RelativityScriptInput("FolderPath", "DocumentFolderPath");

					try
					{
						var scriptResult = client.Repositories.RelativityScript.ExecuteRelativityScript(relativityScript, new List<RelativityScriptInput>() { inputParameter }); 

						if (!scriptResult.Success)
						{
							Log.Error(@"Execution of Relativity Script failed: {0}", scriptResult.Message);
							return false;
						}
					}
					catch (Exception ex)
					{
						Log.Error(ex, @"An error occurred during executing Relativity Script: {0}", ex.Message);
						return false;
					}
				}
			}
			return true;
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

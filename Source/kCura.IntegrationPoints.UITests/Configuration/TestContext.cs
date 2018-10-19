using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using kCura.IntegrationPoints.UITests.Logging;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.UITests.Common;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using NUnit.Framework;
using Relativity.Core;
using Relativity.Services.Objects.DataContracts;
using Serilog;
using Serilog.Events;
using ArtifactType = Relativity.ArtifactType;
using Field = kCura.Relativity.Client.DTOs.Field;
using Group = kCura.IntegrationPoint.Tests.Core.Group;
using UserService = kCura.IntegrationPoint.Tests.Core.User;
using Workspace = kCura.IntegrationPoint.Tests.Core.Workspace;

namespace kCura.IntegrationPoints.UITests.Configuration
{
	public class TestContext
	{

		private const int _ADMIN_USER_ID = 9;
		
		private const string _RIP_GUID_STRING = Core.Constants.IntegrationPoints.APPLICATION_GUID_STRING;

		private const string _LEGAL_HOLD_GUID_STRING = "98F31698-90A0-4EAD-87E3-DAC723FED2A6";
		
		private readonly Lazy<ITestHelper> _helper;

		private readonly string _timeStamp;

		private static readonly ILogger Log = LoggerFactory.CreateLogger(typeof(TestContext));

		
		public ITestHelper Helper => _helper.Value;

		public int? WorkspaceId { get; set; }

		public string WorkspaceName { get; set; }

		public int? GroupId { get; private set; }

		public UserModel User { get; private set; }

		public int? ProductionId { get; private set; }

		public TestContext()
		{
			_helper = new Lazy<ITestHelper>(() => new TestHelper());
			_timeStamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss.ffff");
		}

		public TestContext CreateTestWorkspace()
		{
			CreateTestWorkspaceAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			return this;
		}

		public async Task CreateTestWorkspaceAsync()
		{
			WorkspaceName = $"RIP Test Workspace {_timeStamp}";
			string templateWorkspaceName = SharedVariables.UiTemplateWorkspace;
			Log.Information($"Attempting to create workspace '{WorkspaceName}' using template '{templateWorkspaceName}'.");
			try
			{
				WorkspaceId = await Workspace.CreateWorkspaceAsync(WorkspaceName, templateWorkspaceName, Log).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				Log.Error(ex,
					$"Cannot create workspace '{WorkspaceName}' using template '{templateWorkspaceName}'. Check if Relativity works correctly (services, ...).");
				throw;
			}

			Log.Information($"Workspace '{WorkspaceName}' was successfully created using template '{templateWorkspaceName}'. WorkspaceId={WorkspaceId}");
		}

		public void EnableDataGrid(params string[] fieldNames)
		{
			Workspace.EnableDataGrid(GetWorkspaceId());

			//TODO change implementation to IFieldManager Kepler service
			//ChangeFieldToDataGrid(fieldNames);
		}

		public TestContext CreateUser()
		{
			//GroupId = Group.CreateGroup($"TestGroup_{TimeStamp}");
			//Group.AddGroupToWorkspace(GetWorkspaceId(), GetGroupId());

			ClaimsPrincipal.ClaimsPrincipalSelector += () =>
			{
				var factory = new ClaimsPrincipalFactory();
				return factory.CreateClaimsPrincipal2(_ADMIN_USER_ID, Helper);
			};

			User = UserService.CreateUser("RIP", $"Test_User_{_timeStamp}", $"RIP_Test_User_{_timeStamp}@relativity.com");
			return this;
		}

		public TestContext CreateProductionSet(string productionName)
		{
			var workspaceService = new WorkspaceService(new ImportHelper());
			workspaceService.CreateProductionSet(GetWorkspaceId(), productionName);
			return this;
		}

		public TestContext CreateAndRunProduction(string productionName)
		{
			var workspaceService = new WorkspaceService(new ImportHelper());
			int savedSearchId = workspaceService.CreateSavedSearch(new[] { "Control Number" }, GetWorkspaceId(), $"ForProduction_{productionName}");

			string placeHolderFilePath = Path.Combine(NUnit.Framework.TestContext.CurrentContext.TestDirectory, @"TestData\DefaultPlaceholder.tif");

			int productionId = workspaceService.CreateAndRunProduction(GetWorkspaceId(), savedSearchId, productionName, placeHolderFilePath);

			ProductionId = productionId;

			return this;
		}

		public TestContext InstallIntegrationPoints()
		{
			return InstallApplication(_RIP_GUID_STRING, "Integration Points");
		}

		public TestContext InstallLegalHold()
		{
			return InstallApplication(_LEGAL_HOLD_GUID_STRING, "Legal Hold");
		}

		public TestContext InstallApplication(string guid, string name)
		{
			Assert.NotNull(WorkspaceId, $"{nameof(WorkspaceId)} is null. Was workspace created correctly?.");

			Log.Information("Checking application '{AppName}' ({AppGUID}) in workspace '{WorkspaceName}' ({WorkspaceId}).", name, guid, WorkspaceName, WorkspaceId);
			Stopwatch stopwatch = Stopwatch.StartNew();
			try
			{
				ICoreContext coreContext = SourceProviderTemplate.GetBaseServiceContext(-1);

				var ipAppManager = new RelativityApplicationManager(coreContext, Helper);
				bool isAppInstalledAndUpToDate = ipAppManager.IsApplicationInstalledAndUpToDate((int)WorkspaceId, guid);
				if (!isAppInstalledAndUpToDate)
				{
					Log.Information("Installing application '{AppName}' ({AppGUID}) in workspace '{WorkspaceName}' ({WorkspaceId}).", name, guid, WorkspaceName, WorkspaceId);
					ipAppManager.InstallApplicationFromLibrary((int)WorkspaceId, guid);
					Log.Information("Application '{AppName}' ({AppGUID}) has been installed in workspace '{WorkspaceName}' ({WorkspaceId}) after {AppInstallTime} seconds.", name, guid, WorkspaceName, WorkspaceId, stopwatch.Elapsed.Seconds);
				}
				else
				{
					Log.Information("Application '{AppName}' ({AppGUID}) is already installed in workspace '{WorkspaceName}' ({WorkspaceId}).", name, guid, WorkspaceName, WorkspaceId);
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Detecting or installing application '{AppName}' ({AppGUID}) in the workspace '{WorkspaceName}' ({WorkspaceId}) failed.", name, guid, WorkspaceName, WorkspaceId);
				throw;
			}
			return this;
		}

		public async Task InstallIntegrationPointsAsync()
		{
			await Task.Run(() => InstallIntegrationPoints());
		}

		public async Task InstallLegalHoldAsync()
		{
			await Task.Run(() => InstallLegalHold());
		}


		// TODO fold
		public TestContext ImportDocumentsToRoot(DocumentTestDataBuilder.TestDataType testDataType = DocumentTestDataBuilder.TestDataType.ModerateWithoutFoldersStructure)
		{
			ImportDocuments(true, testDataType);
			return this;
		}

		public TestContext ImportDocumentsToRootWithoutNatives(DocumentTestDataBuilder.TestDataType testDataType = DocumentTestDataBuilder.TestDataType.ModerateWithoutFoldersStructure)
		{
			ImportDocuments(false, testDataType);
			return this;
		}

		public TestContext ImportDocumentsWithoutNatives(DocumentTestDataBuilder.TestDataType testDataType = DocumentTestDataBuilder.TestDataType.ModerateWithFoldersStructure)
		{
			ImportDocuments(false, testDataType);
			return this;
		}

		public TestContext ImportDocumentsWithLargeText(DocumentTestDataBuilder.TestDataType testDataType = DocumentTestDataBuilder.TestDataType.TextWithoutFolderStructure)
		{
			ImportDocuments(false, testDataType);
			return this;
		}

		public async Task ImportDocumentsWithLargeTextAsync(DocumentTestDataBuilder.TestDataType testDataType = DocumentTestDataBuilder.TestDataType.TextWithoutFolderStructure)
		{
			await ImportDocumentsAsync(false, testDataType);
		}
		
		public TestContext ImportDocuments(bool withNatives = true, DocumentTestDataBuilder.TestDataType testDataType = DocumentTestDataBuilder.TestDataType.ModerateWithFoldersStructure)
		{
			Log.Information(@"Importing documents...");
			string testDir = NUnit.Framework.TestContext.CurrentContext.TestDirectory.Replace("kCura.IntegrationPoints.UITests",
				"kCura.IntegrationPoint.Tests.Core");
			Log.Information("TestDir for ImportDocuments '{testDir}'", testDir);
			DocumentsTestData data = DocumentTestDataBuilder.BuildTestData(testDir, withNatives, testDataType);
			var importHelper = new ImportHelper();
			var workspaceService = new WorkspaceService(importHelper);
			bool importSucceded = workspaceService.ImportData(GetWorkspaceId(), data);
			if (importSucceded)
			{
				if (Log.IsEnabled(LogEventLevel.Information))
				{
					string suffix = importHelper.Messages.Any() ? " Messages: " + string.Join("; ", importHelper.Messages) : " No messages.";
					Log.Information(@"Documents imported." + suffix);
				}
			}
			else
			{
				string suffix = importHelper.ErrorMessages.Any() ? " Error messages: " + string.Join("; ", importHelper.ErrorMessages) : " No error messages.";
				throw new UiTestException("Import of documents failed." + suffix);
			}

			return this;
		}

		public TestContext CreateAndRunProduction(string savedSearchName, string productionName)
		{
			var workspaceService = new WorkspaceService(new ImportHelper());
			int savedSearchId = RetrieveSavedSearchId(savedSearchName);
			workspaceService.CreateAndRunProduction(GetWorkspaceId(), savedSearchId, productionName);

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
				QueryResultSet<RelativityScript> relScriptQueryResults = proxy.Repositories.RelativityScript.Query(relScriptQuery);

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
				client.APIOptions.WorkspaceID = GetWorkspaceId();

				RelativityScript relativityScript = FindRelativityFolderPathScript(client);
				Assert.That(relativityScript, Is.Not.Null, "Cannot find Relativity Script to set folder paths");
				
				var inputParameter = new RelativityScriptInput("FolderPath", "DocumentFolderPath");

				try
				{
					RelativityScriptResult scriptResult = client.Repositories.RelativityScript.ExecuteRelativityScript(relativityScript, new List<RelativityScriptInput>() { inputParameter });

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
			return true;
		}

		public bool ChangeFieldToDataGrid(params string[] fieldNames)
		{
			using (var client = Helper.CreateAdminProxy<IRSAPIClient>())
			{
				client.APIOptions.WorkspaceID = WorkspaceId.GetValueOrDefault();

				foreach (string fieldName in fieldNames)
				{
					if (!EnableDataGridOnField(fieldName, client))
					{
						return false;
					}
				}

			}
			return true;
		}

		private static bool EnableDataGridOnField(string fieldName, IRSAPIClient client)
		{
			TextCondition nameCondition = new TextCondition(FieldFieldNames.Name, TextConditionEnum.EqualTo, fieldName);

			Query<Field> fieldQuery = new Query<Field>()
			{
				Condition = nameCondition,
				Fields = FieldValue.AllFields
			};

			try
			{
				var queryResult = client.Repositories.Field.Query(fieldQuery);

				if (!queryResult.Success)
				{
					Log.Error(@"Unable to query Relativity field: {0}", queryResult.Message);
					return false;
				}

				Field field = queryResult.Results[0].Artifact;

				field.EnableDataGrid = true;

				var updateResult = client.Repositories.Field.Update(new List<Field> { field });

				if (!updateResult.Success)
				{
					Log.Error(@"Unable to update Relativity field: {0}", updateResult.Message);
					return false;
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, @"An error occurred during querying or updating Relativity field: {0}", ex.Message);
				return false;
			}

			return true;
		}

		public TestContext TearDown()
		{
			if (GroupId.HasValue)
			{
				Group.DeleteGroup(GetGroupId());
			}

			if (User != null)
			{
				UserService.DeleteUser(User.ArtifactId);
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
			Assert.AreNotEqual(0, WorkspaceId, $"{nameof(WorkspaceId)} is 0. Workspace wasn't created correctly.");
			return WorkspaceId.Value;
		}

		private int RetrieveSavedSearchId(string savedSearchName)
		{
			var objectManager = new RelativityObjectManager(WorkspaceId.Value, Helper, null); // we don't need secret store helper to read saved search
			var savedSearchRequest = new QueryRequest
			{
				ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.Search },
				Condition = $"'Name' == '{savedSearchName}'",
				Fields = new FieldRef[0]
			};
			RelativityObject savedSearch = objectManager.Query(savedSearchRequest).First();
			return savedSearch.ArtifactID;
		}
	}

}

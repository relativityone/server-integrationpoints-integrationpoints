﻿using System;
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
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using NUnit.Framework;
using Relativity.Core;
using Relativity.Services.Objects.DataContracts;
using Serilog;
using ArtifactType = Relativity.ArtifactType;
using Field = kCura.Relativity.Client.DTOs.Field;
using Group = kCura.IntegrationPoint.Tests.Core.Group;
using User = kCura.IntegrationPoint.Tests.Core.User;
using Workspace = kCura.IntegrationPoint.Tests.Core.Workspace;

namespace kCura.IntegrationPoints.UITests.Configuration
{
	public class TestContext
	{

		private const int _ADMIN_USER_ID = 9;
		private const string _TEMPLATE_WKSP_NAME = "Smoke TestCase";
		private const string _RIP_GUID_STRING = Core.Constants.IntegrationPoints.APPLICATION_GUID_STRING;
		private const string _LEGAL_HOLD_GUID_STRING = "98F31698-90A0-4EAD-87E3-DAC723FED2A6";
		private const string _RELATIVITY_STARTER_TEMPLATE = "Relativity Starter Template";
		private readonly Lazy<ITestHelper> _helper;

		private static readonly ILogger Log = LoggerFactory.CreateLogger(typeof(TestContext));

		public readonly string TimeStamp;

		public ITestHelper Helper => _helper.Value;

		public int? WorkspaceId { get; set; }

		public string WorkspaceName { get; set; }

		public int? GroupId { get; private set; }

		public int? UserId { get; private set; }

		public int? ProductionId { get; private set; }

		public TestContext()
		{
			_helper = new Lazy<ITestHelper>(() => new TestHelper());
			TimeStamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
		}

		public TestContext CreateTestWorkspace()
		{
			CreateTestWorkspaceAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			return this;
		}

		public async Task CreateTestWorkspaceAsync()
		{
			if (!Workspace.IsWorkspacePresent(_TEMPLATE_WKSP_NAME))
			{
				await Workspace.CreateWorkspaceAsync(_TEMPLATE_WKSP_NAME, _RELATIVITY_STARTER_TEMPLATE);
			}

			WorkspaceName = $"1A Test Workspace {TimeStamp}";
			Log.Information($"Attempting to create workspace '{WorkspaceName}' using template '{_TEMPLATE_WKSP_NAME}'.");
			try
			{
				WorkspaceId = await Workspace.CreateWorkspaceAsync(WorkspaceName, _TEMPLATE_WKSP_NAME);
			}
			catch (Exception ex)
			{
				Log.Error(ex,
					$"Cannot create workspace '{WorkspaceName}' using template '{_TEMPLATE_WKSP_NAME}'. Check if Relativity works correctly (services, ...).");
				throw;
			}

			Log.Information($"Workspace '{WorkspaceName}' was successfully created using template '{_TEMPLATE_WKSP_NAME}.");
		}

		public void EnableDataGrid(params string[] fieldNames)
		{
			Workspace.EnableDataGrid(GetWorkspaceId());

			//TODO change implementation to IFieldManager Kepler service
			//ChangeFieldToDataGrid(fieldNames);
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

		public TestContext CreateProductionSet(string productionName)
		{
			var workspaceService = new WorkspaceService(new ImportHelper());
			workspaceService.CreateProductionSet(GetWorkspaceId(), productionName);
			return this;
		}

		public TestContext CreateAndRunProduction(string productionName)
		{
			var workspaceService = new WorkspaceService(new ImportHelper());
			int savedSearchId = workspaceService.CreateSavedSearch(new string[] { "Control Number" }, GetWorkspaceId(), $"ForProduction_{productionName}");

			string placeHolderFilePath = Path.Combine(NUnit.Framework.TestContext.CurrentContext.TestDirectory, @"TestData\DefaultPlaceholder.tif");

			int productionId = workspaceService.CreateAndRunProduction(GetWorkspaceId(), savedSearchId, productionName, placeHolderFilePath);

			ProductionId = productionId;

			return this;
		}

		public TestContext InstallIntegrationPoints()
		{
			return InstallApplication(_RIP_GUID_STRING);
		}

		public TestContext InstallLegalHold()
		{
			return InstallApplication(_LEGAL_HOLD_GUID_STRING);
		}

		public TestContext InstallApplication(string guid)
		{
			Assert.NotNull(WorkspaceId, $"{nameof(WorkspaceId)} is null. Workspace wasn't created.");

			Stopwatch stopwatch = Stopwatch.StartNew();
			try
			{
				ICoreContext coreContext = SourceProviderTemplate.GetBaseServiceContext(-1);

				var ipAppManager = new RelativityApplicationManager(coreContext, Helper);
				bool isAppInstalledAndUpToDate = ipAppManager.IsApplicationInstalledAndUpToDate((int)WorkspaceId, guid);
				if (!isAppInstalledAndUpToDate)
				{
					ipAppManager.InstallApplicationFromLibrary((int)WorkspaceId, guid);
				}
				Log.Information(@"Application is installed.");
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Unexpected error when detecting or installing application in the workspace. Application guid: {guid}");
				throw;
			}
			finally
			{
				Log.Information($"Installation of application with guid {guid} in workspace took {stopwatch.Elapsed.Seconds} seconds.");
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
			DocumentsTestData data = DocumentTestDataBuilder.BuildTestData(testDir, withNatives, testDataType);
			var workspaceService = new WorkspaceService(new ImportHelper());
			workspaceService.ImportData(GetWorkspaceId(), data);
			Log.Information(@"Documents imported.");
			return this;
		}

		public TestContext CreateAndRunProduction(string savedSearchName, string productionName)
		{
			var workspaceService = new WorkspaceService(new ImportHelper());
			int savedSearchId = RetrieveSavedSearchId(savedSearchName);
			workspaceService.CreateAndRunProduction(WorkspaceId.Value, savedSearchId, productionName);

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
				client.APIOptions.WorkspaceID = GetWorkspaceId();

				var relativityScript = FindRelativityFolderPathScript(client);
				Assert.That(relativityScript, Is.Not.Null, "Cannot find Relativity Script to set folder paths");

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

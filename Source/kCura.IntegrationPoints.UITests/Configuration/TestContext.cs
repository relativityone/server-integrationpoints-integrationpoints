using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.UITests.Configuration.Helpers;
using kCura.IntegrationPoints.UITests.Logging;
using NUnit.Framework;
using Serilog;
using System;
using System.Threading.Tasks;
using Workspace = kCura.IntegrationPoint.Tests.Core.Workspace;

namespace kCura.IntegrationPoints.UITests.Configuration
{
	public class TestContext
	{
		private readonly string _timeStamp;

		private readonly Lazy<ITestHelper> _helperLazy;

		private readonly Lazy<IRelativityObjectManager> _objectManagerLazy;

		private readonly Lazy<ApplicationInstallationHelper> _applicationInstallationHelperLazy;

		private readonly Lazy<EntityObjectTypeHelper> _entityObjectTypeHelperLazy;

		private readonly Lazy<RelativityFolderPathScriptHelper> _relativityFolderPathScriptHelperLazy;

		private readonly Lazy<ProductionHelper> _productionHelperLazy;

		private readonly Lazy<ImportDocumentsHelper> _importDocumentHelper;

		private readonly Lazy<UserHelper> _userHelperLazy;

		private static readonly ILogger Log = LoggerFactory.CreateLogger(typeof(TestContext));

		public ITestHelper Helper => _helperLazy.Value;

		internal IRelativityObjectManager ObjectManager => _objectManagerLazy.Value;

		internal ApplicationInstallationHelper ApplicationInstallationHelper => _applicationInstallationHelperLazy.Value;

		internal EntityObjectTypeHelper EntityObjectTypeHelper => _entityObjectTypeHelperLazy.Value;

		internal RelativityFolderPathScriptHelper RelativityFolderPathScriptHelper => _relativityFolderPathScriptHelperLazy.Value;

		internal ProductionHelper ProductionHelper => _productionHelperLazy.Value;

		internal ImportDocumentsHelper ImportDocumentsHelper => _importDocumentHelper.Value;

		internal UserHelper UserHelper => _userHelperLazy.Value;

		public int? WorkspaceId { get; set; }

		public string WorkspaceName { get; set; }

		public RelativityUser User { get; private set; }

		public TestContext()
		{
			_timeStamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss.ffff");

			_helperLazy = new Lazy<ITestHelper>(
				() => new TestHelper()
			);
			_objectManagerLazy = new Lazy<IRelativityObjectManager>(CreateObjectManager);

			_applicationInstallationHelperLazy = new Lazy<ApplicationInstallationHelper>(
				() => new ApplicationInstallationHelper(this)
			);
			_entityObjectTypeHelperLazy = new Lazy<EntityObjectTypeHelper>(
				() => new EntityObjectTypeHelper(this)
			);
			_relativityFolderPathScriptHelperLazy = new Lazy<RelativityFolderPathScriptHelper>(
				() => new RelativityFolderPathScriptHelper(this)
			);
			_productionHelperLazy = new Lazy<ProductionHelper>(
				() => new ProductionHelper(this)
			);
			_importDocumentHelper = new Lazy<ImportDocumentsHelper>(
				() => new ImportDocumentsHelper(this)
			);
			_userHelperLazy = new Lazy<UserHelper>(
				() => new UserHelper(this)
			);
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

			// TODO change implementation to IFieldManager Kepler service
		}

		public TestContext InitUser()
		{
			User = UserHelper.GetOrCreateTestUser(_timeStamp);
			return this;
		}

		public TestContext CreateProductionSet(string productionName)
		{
			ProductionHelper.CreateProductionSet(productionName);
			return this;
		}

		public TestContext CreateAndRunProduction(string productionName)
		{
			ProductionHelper.CreateAndRunProduction(productionName);
			return this;
		}

		public TestContext CreateAndRunProduction(string savedSearchName, string productionName)
		{
			ProductionHelper.CreateAndRunProduction(savedSearchName, productionName);
			return this;
		}

		public TestContext AddEntityObjectToWorkspace()
		{
			EntityObjectTypeHelper.AddEntityObjectToWorkspace();
			return this;
		}

		public Task CreateEntityView(string viewName)
		{
			return EntityObjectTypeHelper.CreateEntityView(viewName);
		}

		public async Task InstallIntegrationPointsAsync()
		{
			await Task.Run(() =>
				ApplicationInstallationHelper.InstallIntegrationPoints()
			).ConfigureAwait(false);
		}

		public TestContext ImportDocumentsToRoot()
		{
			ImportDocuments(
				withNatives: true,
				testDataType: DocumentTestDataBuilder.TestDataType.ModerateWithoutFoldersStructure
			);
			return this;
		}

		public TestContext ImportDocumentsToRootWithoutNatives()
		{
			ImportDocuments(
				withNatives: false,
				testDataType: DocumentTestDataBuilder.TestDataType.ModerateWithoutFoldersStructure
			);
			return this;
		}

		public TestContext ImportDocumentsWithoutNatives()
		{
			ImportDocuments(
				withNatives: false,
				testDataType: DocumentTestDataBuilder.TestDataType.ModerateWithFoldersStructure
			);
			return this;
		}

		public async Task ImportDocumentsWithLargeTextAsync()
		{
			await ImportDocumentsAsync(
				withNatives: false,
				testDataType: DocumentTestDataBuilder.TestDataType.TextWithoutFolderStructure
			);
		}

		public TestContext ImportDocuments(bool withNatives = true, DocumentTestDataBuilder.TestDataType testDataType = DocumentTestDataBuilder.TestDataType.ModerateWithFoldersStructure)
		{
			ImportDocumentsHelper.ImportDocuments(withNatives, testDataType);
			return this;
		}

		public async Task ImportDocumentsAsync(bool withNatives = true, DocumentTestDataBuilder.TestDataType testDataType = DocumentTestDataBuilder.TestDataType.ModerateWithFoldersStructure)
		{
			await Task.Run(() => ImportDocuments(withNatives, testDataType));
		}

		public bool ExecuteRelativityFolderPathScript()
		{
			return RelativityFolderPathScriptHelper.ExecuteRelativityFolderPathScript();
		}

		public TestContext TearDown()
		{
			if (_userHelperLazy.IsValueCreated)
			{
				_userHelperLazy.Value.DeleteUserIfWasCreated();
			}
			return this;
		}

		public int GetWorkspaceId()
		{
			Assert.NotNull(WorkspaceId, $"{nameof(WorkspaceId)} is null. Workspace wasn't created.");
			Assert.AreNotEqual(0, WorkspaceId, $"{nameof(WorkspaceId)} is 0. Workspace wasn't created correctly.");
			return WorkspaceId.Value;
		}

		private IRelativityObjectManager CreateObjectManager()
		{
			var factory = new RelativityObjectManagerFactory(Helper);
			return factory.CreateRelativityObjectManager(WorkspaceId.Value);
		}
	}
}

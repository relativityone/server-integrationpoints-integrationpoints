using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.UITests.Configuration.Helpers;
using kCura.IntegrationPoints.UITests.Logging;
using NUnit.Framework;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.Productions;
using kCura.IntegrationPoints.UITests.Configuration.Models;
using Relativity;
using Relativity.Services.Interfaces.Field;
using Relativity.Services.Interfaces.Field.Models;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Workspace;
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
		
		private readonly Lazy<FieldMappingHelper> _workspaceFieldMappingHelperLazy;

		private static readonly ILogger Log = LoggerFactory.CreateLogger(typeof(TestContext));

		public const DocumentTestDataBuilder.TestDataType DEFAULT_IMPORT_TEST_DATATYPE = DocumentTestDataBuilder.TestDataType.SmallWithFoldersStructure;

		public const DocumentTestDataBuilder.TestDataType DEFAULT_IMPORT_TEST_DATATYPE_WITHOUT_FOLDERS = DocumentTestDataBuilder.TestDataType.SmallWithoutFolderStructure;

		public ITestHelper Helper => _helperLazy.Value;

		internal IRelativityObjectManager ObjectManager => _objectManagerLazy.Value;

		internal ApplicationInstallationHelper ApplicationInstallationHelper => _applicationInstallationHelperLazy.Value;

		internal EntityObjectTypeHelper EntityObjectTypeHelper => _entityObjectTypeHelperLazy.Value;

		internal RelativityFolderPathScriptHelper RelativityFolderPathScriptHelper => _relativityFolderPathScriptHelperLazy.Value;

		internal ProductionHelper ProductionHelper => _productionHelperLazy.Value;

		internal ImportDocumentsHelper ImportDocumentsHelper => _importDocumentHelper.Value;

		internal UserHelper UserHelper => _userHelperLazy.Value;
		internal FieldMappingHelper WorkspaceFieldMappingHelper => _workspaceFieldMappingHelperLazy.Value;

		public int? WorkspaceId { get; set; }

		public string WorkspaceName { get; set; }

		public RelativityUser User { get; private set; }

		public List<FieldObject> WorkspaceMappableFields { get; set; }
		public List<FieldObject> WorkspaceAutoMapAllEnabledFields { get; set; }

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
				() => new ProductionHelper(GetWorkspaceId())
			);
			_importDocumentHelper = new Lazy<ImportDocumentsHelper>(
				() => new ImportDocumentsHelper(this)
			);
			_userHelperLazy = new Lazy<UserHelper>(
				() => new UserHelper(this)
			);
			_workspaceFieldMappingHelperLazy = new Lazy<FieldMappingHelper>(
				() => new FieldMappingHelper(this)
			);
		}

		public async Task CreateTestWorkspaceAsync(string suffix = "")
		{
			var createDurationStopWatch = new Stopwatch();
			createDurationStopWatch.Start();
			WorkspaceName = $"RIP Test Workspace {_timeStamp}" + suffix;
			string templateWorkspaceName = SharedVariables.UiTemplateWorkspace;
			Log.Information($"Attempting to create workspace '{WorkspaceName}' using template '{templateWorkspaceName}'.");
			try
			{
				WorkspaceRef workspace = await Workspace.CreateWorkspaceAsync(WorkspaceName, templateWorkspaceName).ConfigureAwait(false);
				WorkspaceId = workspace.ArtifactID;
			}
			catch (Exception ex)
			{
				Log.Error(ex,
					$"Cannot create workspace '{WorkspaceName}' using template '{templateWorkspaceName}'. Check if Relativity works correctly (services, ...).");
				throw;
			}
			createDurationStopWatch.Stop();
			Log.Information($"Workspace '{WorkspaceName}' was successfully created using template '{templateWorkspaceName}'. WorkspaceId={WorkspaceId}");
			Log.Information("Workspace created. Duration: {duration} s", createDurationStopWatch.ElapsedMilliseconds/1000 );
		}

		public async Task EnableDataGridForFieldAsync(string fieldName)
		{
			int workspaceId = GetWorkspaceId();
			using (IObjectManager objectManager = Helper.CreateProxy<IObjectManager>())
			using (IFieldManager fieldManager = Helper.CreateProxy<IFieldManager>())
			{
				QueryRequest fieldRequest = Fields.CreateObjectManagerArtifactIdQueryRequest(fieldName);
				QueryResult fieldQueryResult = await objectManager.QueryAsync(workspaceId, fieldRequest, 0, 1).ConfigureAwait(false);
				int fieldArtifactId = fieldQueryResult.Objects.FirstOrDefault().ArtifactID;
			
				var enableDataGridOnLongTextFieldRequest = new LongTextFieldRequest()
				{
					ObjectType = new ObjectTypeIdentifier()
					{
						ArtifactTypeID = (int)ArtifactType.Document
					},
					Name = $"{fieldName}",
					EnableDataGrid = true,
					IncludeInTextIndex = false,
					FilterType = FilterType.None,
					AvailableInViewer = true,
					HasUnicode = true
				};
				await fieldManager.UpdateLongTextFieldAsync(workspaceId, fieldArtifactId, enableDataGridOnLongTextFieldRequest).ConfigureAwait(false);
			}
		}

		public TestContext InitUser()
		{
			User = UserHelper.GetOrCreateTestUser(_timeStamp);
			return this;
		}

		public TestContext CreateProductionSet(string productionName)
		{
			Log.Information($"Create production set {productionName}");
			var sw = new Stopwatch();
			sw.Start();
			ProductionHelper.CreateProductionSet(productionName);
			sw.Stop();
			Log.Information("Production set created. Duration {productionName} s", sw.ElapsedMilliseconds / 1000);
			return this;
		}

		public TestContext CreateProductionAndImportData(string productionName)
		{
			ProductionHelper.CreateProductionSetAndImportData(productionName, DEFAULT_IMPORT_TEST_DATATYPE);
			return this;
		}

		public async Task<TestContext> AddEntityObjectToWorkspaceAsync()
		{
			await EntityObjectTypeHelper.AddEntityObjectToWorkspaceAsync().ConfigureAwait(false);
			return this;
		}

		public Task CreateEntityViewAsync(string viewName)
		{
			return EntityObjectTypeHelper.CreateEntityView(viewName);
		}

		public Task<bool> IsIntegrationPointsInstalledAsync()
		{
			return ApplicationInstallationHelper.IsIntegrationPointsInstalledAsync();
		}

		public TestContext ImportDocumentsToRoot()
		{
			ImportDocuments(
				withNatives: true,
				testDataType: DEFAULT_IMPORT_TEST_DATATYPE_WITHOUT_FOLDERS
			);
			return this;
		}

		public TestContext ImportDocumentsToRootWithoutNatives()
		{
			ImportDocuments(
				withNatives: false,
				testDataType: DEFAULT_IMPORT_TEST_DATATYPE_WITHOUT_FOLDERS
			);
			return this;
		}

		public TestContext ImportDocumentsWithoutNatives()
		{
			ImportDocuments(
				withNatives: false,
				testDataType: DEFAULT_IMPORT_TEST_DATATYPE_WITHOUT_FOLDERS
			);
			return this;
		}

		public Task ImportDocumentsWithLargeTextAsync()
		{
			return ImportDocumentsAsync(
				withNatives: false,
				testDataType: DocumentTestDataBuilder.TestDataType.TextWithoutFolderStructure
			);
		}

		public TestContext ImportDocuments(bool withNatives = true, DocumentTestDataBuilder.TestDataType testDataType = DEFAULT_IMPORT_TEST_DATATYPE)
		{
			ImportDocumentsHelper.ImportDocuments(withNatives, testDataType);
			return this;
		}

		public Task ImportDocumentsAsync(bool withNatives = true, DocumentTestDataBuilder.TestDataType testDataType = DEFAULT_IMPORT_TEST_DATATYPE)
		{
			return Task.Run(() => ImportDocuments(withNatives, testDataType));
		}

		public TestContext TearDown()
		{
			if (_userHelperLazy.IsValueCreated)
			{
				_userHelperLazy.Value.DeleteUserIfWasCreated();
			}
			return this;
		}

		public async Task<TestContext> RetrieveMappableFieldsAsync()
		{
			WorkspaceMappableFields = await WorkspaceFieldMappingHelper.GetFilteredDocumentsFieldsFromWorkspaceAsync().ConfigureAwait(false);
            WorkspaceAutoMapAllEnabledFields = await WorkspaceFieldMappingHelper.GetAutoMapAllEnabledFieldsAsync().ConfigureAwait(false);
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

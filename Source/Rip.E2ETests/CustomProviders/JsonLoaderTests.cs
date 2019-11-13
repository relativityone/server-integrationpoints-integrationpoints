using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Constants;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using NUnit.Framework;
using Relativity.IntegrationPoints.Services;
using Relativity.Services.Folder;
using Relativity.Services.Objects.DataContracts;
using Relativity.Testing.Identification;
using Rip.E2ETests.Constants;
using Rip.E2ETests.CustomProviders.Arrangers;
using Rip.E2ETests.CustomProviders.Helpers;
using Rip.E2ETests.CustomProviders.TestCases;
using CoreConstants = kCura.IntegrationPoints.Core.Constants;

namespace Rip.E2ETests.CustomProviders
{
	[TestFixture]
	public class JsonLoaderTests
	{
		private const string _WORKSPACE_TEMPLATE_WITHOUT_RIP = WorkspaceTemplateNames.NEW_CASE_TEMPLATE_NAME;

		private int _workspaceID;
		private int _jsonLoaderArtifactID;
		private int _relativityDestinationProviderArtifactID;
		private int _targetObjectTypeArtifactID;
		private int _importIntegrationPointTypeArtifactID;
		private int _rootFolderID;
		private int _appendOverlayChoiceArtifactID;

		private IRelativityObjectManager _objectManager;

		private ITestHelper TestHelper { get; }
		private RelativityApplicationManager ApplicationManager { get; }

		public JsonLoaderTests()
		{
			TestHelper = new TestHelper();
			ApplicationManager = new RelativityApplicationManager(TestHelper);
		}
        
		[OneTimeSetUp]
		public async Task OneTimeSetUpAsync()
		{
			await CreateWorkspaceWithJsonLoaderAsync().ConfigureAwait(false);
			await Agent.CreateIntegrationPointAgentIfNotExistsAsync().ConfigureAwait(false);
			await Agent.EnableAllIntegrationPointsAgentsAsync().ConfigureAwait(false);
			var objectManagerFactory = new RelativityObjectManagerFactory(TestHelper);
			_objectManager = objectManagerFactory.CreateRelativityObjectManager(_workspaceID);

			await InitializeWorkspaceSpecificIdentifiersAsync().ConfigureAwait(false);
		}

		[TearDown]
		public void OneTimeTearDown()
		{
			Workspace.DeleteWorkspace(_workspaceID);
		}

		[IdentifiedTest("197e67c7-4841-44e5-8169-fa51f2a157b6")]
		public async Task ShouldImportDocumentsUsingJsonLoaderTest()
		{
			// Arrange
			CustomProviderTestCase testCase = JsonLoaderTestCaseArranger.GetTestCase(_workspaceID);
			int integrationPointID = await CreateJsonLoaderIntegrationPointAsync(testCase).ConfigureAwait(false);

			// Act
			await IntegrationPointsTestHelper
				.RunIntegrationPointAsync(
					TestHelper,
					_workspaceID,
					integrationPointID)
				.ConfigureAwait(false);

			// Assert
			await AssertJobHistoryIsValidAsync(integrationPointID, testCase).ConfigureAwait(false);
			await AssertDocumentsAreImportedAsync(testCase).ConfigureAwait(false);
		}

		private async Task<int> CreateJsonLoaderIntegrationPointAsync(CustomProviderTestCase testCase)
		{
			CreateIntegrationPointRequest integrationPointCreateRequest = await BuildCreateIntegrationPointRequestAsync(testCase).ConfigureAwait(false);
			return await IntegrationPointsTestHelper
				.SaveIntegrationPointAsync(
					TestHelper,
					_objectManager,
					integrationPointCreateRequest,
					testCase)
				.ConfigureAwait(false);
		}

		private async Task<CreateIntegrationPointRequest> BuildCreateIntegrationPointRequestAsync(CustomProviderTestCase testCase)
		{
			_targetObjectTypeArtifactID = await ObjectTypeHelper.GetObjectTypeArtifactIdByNameAsync(_objectManager, testCase.TargetRdoArtifactName)
				.ConfigureAwait(false);

			var destinationConfiguration = new
			{
				artifactTypeID = _targetObjectTypeArtifactID,
				destinationProviderType = CoreConstants.IntegrationPoints.DestinationProviders.RELATIVITY,
				CaseArtifactId = _workspaceID,
				DestinationFolderArtifactId = _rootFolderID,
				ExtractedTextFieldContainsFilePath = false,
				ImportOverwriteMode = "AppendOverlay"
			};

			var sourceConfiguration = new
			{
				fieldLocation = testCase.FieldFilePath,
				dataLocation = testCase.DataFilePath
			};
			List<FieldMap> fieldMapping = await BuildFieldMappingJsonLoaderAsync(testCase).ConfigureAwait(false);

			var integrationPointCreateRequest = new CreateIntegrationPointRequest
			{
				WorkspaceArtifactId = _workspaceID,
				IntegrationPoint = new IntegrationPointModel
				{
					Name = "Import from Json Loader",
					Type = _importIntegrationPointTypeArtifactID,
					SourceProvider = _jsonLoaderArtifactID,
					DestinationProvider = _relativityDestinationProviderArtifactID,
					FieldMappings = fieldMapping,
					SourceConfiguration = sourceConfiguration,
					DestinationConfiguration = destinationConfiguration,
					OverwriteFieldsChoiceId = _appendOverlayChoiceArtifactID,
					ScheduleRule = new ScheduleModel { EnableScheduler = false }
				}
			};
			return integrationPointCreateRequest;
		}

		private async Task<List<FieldMap>> BuildFieldMappingJsonLoaderAsync(CustomProviderTestCase testCase)
		{
			Dictionary<string, int> fieldNamesToArtifactIDMapping = await FieldsTestHelper
							.GetIdentifiersForFieldsAsync(
								_objectManager,
								_targetObjectTypeArtifactID,
								testCase.WorkspaceFieldsNames)
							.ConfigureAwait(false);


			List<FieldMap> fieldMapping = testCase.WorkspaceFieldsToFileFieldsMapping.Select(mapping => new FieldMap
			{
				FieldMapType = mapping.Key == testCase.IdentifierFieldName
					? FieldMapType.Identifier
					: FieldMapType.None,
				SourceField = new FieldEntry
				{
					FieldIdentifier = mapping.Value,
					DisplayName = testCase.SourceFieldFieldIdentifierToDisplayNameMapping[mapping.Value],
					IsIdentifier = mapping.Key == testCase.IdentifierFieldName
				},
				DestinationField = new FieldEntry
				{
					FieldIdentifier = fieldNamesToArtifactIDMapping[mapping.Key].ToString(),
					DisplayName = mapping.Key,
					IsIdentifier = mapping.Key == testCase.IdentifierFieldName
				}
			}).ToList();
			return fieldMapping;
		}

		private async Task AssertJobHistoryIsValidAsync(int integrationPointID, CustomProviderTestCase testCase)
		{
			JobHistory jobHistory = await JobHistoryTestHelper.GetCompletedJobHistoryAsync(_objectManager, integrationPointID, testCase.MaximumExecutionTime).ConfigureAwait(false);

			jobHistory.JobStatus.Name.Should().Be(testCase.ExpectedStatus.Name);
			jobHistory.ItemsTransferred.Should().Be(testCase.ExpectedItemsTransferred);
			jobHistory.TotalItems.Should().Be(testCase.ExpectedTotalItems);
		}

		private async Task AssertDocumentsAreImportedAsync(CustomProviderTestCase testCase) 
		{ 
			IDictionary<string, string> nameToSampleTextDictionary = await DocumentsTestHelper 
				.GetSampleTextForJsonObjectsAsync(
					_objectManager,
					_targetObjectTypeArtifactID,
					testCase.ExpectedDocumentsIdentifiers)
				.ConfigureAwait(false);
 
			foreach (string documentIdentifier in testCase.ExpectedDocumentsIdentifiers) 
			{ 
				nameToSampleTextDictionary.Should().ContainKey(documentIdentifier); 
				string expectedSampleText = testCase.ExpectedDocumentsIdentifiersToExtractedTextMapping[documentIdentifier]; 
				nameToSampleTextDictionary[documentIdentifier].Should().Be(expectedSampleText); 
			} 
		} 

		private Task InitializeWorkspaceSpecificIdentifiersAsync()
		{
			Task[] initializeTasks =
			{
				InitializeJsonLoaderArtifactIDAsync(),
				InitializeRelativityDestinationProviderArtifactIDAsync(),
				InitializeImportIntegrationPointTypeArtifatIDAsync(),
				InitializeAppendOverlayChoiceArtifactIDAsync(),
				InitializeRootFolderArtifactIDAsync()
			};
			return Task.WhenAll(initializeTasks);
		}

		private async Task InitializeJsonLoaderArtifactIDAsync()
		{
			List<SourceProvider> sourceProviders = await _objectManager.QueryAsync<SourceProvider>(new QueryRequest()).ConfigureAwait(false);
			SourceProvider jsonLoaderProvider = sourceProviders.Single(x => x.Name == CustomProvidersConstants.JSON_LOADER_SOURCE_PROVIDER_NAME);
			_jsonLoaderArtifactID = jsonLoaderProvider.ArtifactId;
		}

		private async Task InitializeRelativityDestinationProviderArtifactIDAsync()
		{
			List<DestinationProvider> destinationProviders = await _objectManager.QueryAsync<DestinationProvider>(new QueryRequest()).ConfigureAwait(false);
			DestinationProvider relativityDestinationProvider = destinationProviders.Single(x => x.Name == "Relativity");
			_relativityDestinationProviderArtifactID = relativityDestinationProvider.ArtifactId;
		}

		private async Task InitializeImportIntegrationPointTypeArtifatIDAsync()
		{
			List<IntegrationPointType> integrationPointTypes = await _objectManager.QueryAsync<IntegrationPointType>(new QueryRequest()).ConfigureAwait(false);
			IntegrationPointType importIntegrationPointType = integrationPointTypes.Single(x => x.Name == "Import");
			_importIntegrationPointTypeArtifactID = importIntegrationPointType.ArtifactId;
		}

		private async Task InitializeAppendOverlayChoiceArtifactIDAsync()
		{
			_appendOverlayChoiceArtifactID = await ChoiceTestHelper
				.GetIntegrationPointsChoiceArtifactIDAsync(
					_objectManager,
					OverwriteFieldsChoices.IntegrationPointAppendOverlayGuid)
				.ConfigureAwait(false);
		}

		private async Task InitializeRootFolderArtifactIDAsync()
		{
			using (var folderManager = TestHelper.CreateProxy<IFolderManager>())
			{
				Folder rootFolder = await folderManager
					.GetWorkspaceRootAsync(_workspaceID)
					.ConfigureAwait(false);
				_rootFolderID = rootFolder.ArtifactID;
			}
		}

		private async Task CreateWorkspaceWithJsonLoaderAsync()
		{
			string workspaceName = nameof(JsonLoaderTests);
			_workspaceID = await Workspace.CreateWorkspaceAsync(workspaceName, _WORKSPACE_TEMPLATE_WITHOUT_RIP).ConfigureAwait(false);

			Task importingJsonLoaderToLibraryTask = ImportJsonLoaderToLibraryAsync();
            await ApplicationManager.InstallRipFromLibraryAsync(_workspaceID).ConfigureAwait(false);
            await importingJsonLoaderToLibraryTask.ConfigureAwait(false);
            await ApplicationManager.InstallApplicationFromLibraryAsync(_workspaceID, CustomProvidersConstants.JsonLoaderGuid).ConfigureAwait(false);
        }

		private Task ImportJsonLoaderToLibraryAsync()
		{
			string applicationFilePath = SharedVariables.JsonLoaderRapFilePath;
			return ApplicationManager.ImportApplicationToLibraryAsync(applicationFilePath);
		}
	}
}

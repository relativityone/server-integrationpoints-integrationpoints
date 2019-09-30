using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Constants;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Services;
using NUnit.Framework;
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
	[Feature.DataTransfer.IntegrationPoints]
	public class MyFirstProviderTests
	{
		private const string _WORKSPACE_TEMPLATE_WITHOUT_RIP = WorkspaceTemplateNames.NEW_CASE_TEMPLATE_NAME;

		private int _workspaceID;
		private int _myFirstProviderArtifactID;
		private int _relativityDestinationProviderArtifactID;
		private int _importIntegrationPointTypeArtifactID;
		private int _rootFolderID;
		private int _appendOverlayChoiceArtifactID;

		private IRelativityObjectManager _objectManager;

		private ITestHelper TestHelper { get; }
		private RelativityApplicationManager ApplicationManager { get; }

		public MyFirstProviderTests()
		{
			TestHelper = new TestHelper();
			ApplicationManager = new RelativityApplicationManager(TestHelper);
		}

		[OneTimeSetUp]
		public async Task OneTimeSetUpAsync()
		{
			await CreateWorkspaceWithMyFirstProviderAsync().ConfigureAwait(false);
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

		[IdentifiedTest("5b2b2176-c771-49fa-a273-db33701e954a")]
		public async Task ShouldImportDocumentsUsingMyFirstProvider()
		{
			// Arrange
			MyFirstProviderTestCase testCase = MyFirstProviderTestCaseArranger.GetTestCase(_workspaceID);
			int integrationPointID = await CreateMyFirstProviderIntegrationPointAsync(testCase).ConfigureAwait(false);

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

		private async Task<int> CreateMyFirstProviderIntegrationPointAsync(MyFirstProviderTestCase testCase)
		{
			CreateIntegrationPointRequest integrationPointCreateRequest = await BuildCreateIntegrationPointRequestAsync(testCase).ConfigureAwait(false);
			return await IntegrationPointsTestHelper
				.SaveIntegrationPointAsync(
					TestHelper,
					_objectManager,
					integrationPointCreateRequest)
				.ConfigureAwait(false);
		}

		private async Task<CreateIntegrationPointRequest> BuildCreateIntegrationPointRequestAsync(MyFirstProviderTestCase testCase)
		{
			var destinationConfiguration = new
			{
				artifactTypeID = testCase.TargetRdoArtifactID,
				destinationProviderType = CoreConstants.IntegrationPoints.DestinationProviders.RELATIVITY,
				CaseArtifactId = _workspaceID,
				DestinationFolderArtifactId = _rootFolderID,
				ExtractedTextFieldContainsFilePath = false,
				ImportOverwriteMode = "AppendOverlay"
			};
			List<FieldMap> fieldMapping = await BuildFieldMappingAsync(testCase).ConfigureAwait(false);

			var integrationPointCreateRequest = new CreateIntegrationPointRequest
			{
				WorkspaceArtifactId = _workspaceID,
				IntegrationPoint = new IntegrationPointModel
				{
					Name = "Import from MyFirstProvider",
					Type = _importIntegrationPointTypeArtifactID,
					SourceProvider = _myFirstProviderArtifactID,
					DestinationProvider = _relativityDestinationProviderArtifactID,
					FieldMappings = fieldMapping,
					SourceConfiguration = testCase.InputFilePath,
					DestinationConfiguration = destinationConfiguration,
					OverwriteFieldsChoiceId = _appendOverlayChoiceArtifactID,
					ScheduleRule = new ScheduleModel { EnableScheduler = false }
				}
			};
			return integrationPointCreateRequest;
		}

		private async Task<List<FieldMap>> BuildFieldMappingAsync(MyFirstProviderTestCase testCase)
		{
			Dictionary<string, int> fieldNamesToArtifactIDMapping = await FieldsTestHelper
							.GetIdentifiersForFieldsAsync(
								_objectManager,
								testCase.TargetRdoArtifactID,
								testCase.WorkspaceFieldsNames)
							.ConfigureAwait(false);

			List<FieldMap> fieldMapping = testCase.WorkspaceFieldsToFileFieldsMapping.Select(mapping => new FieldMap
			{
				FieldMapType = mapping.Key == testCase.IdentifierFieldName
					? FieldMapType.Identifier
					: FieldMapType.None,
				SourceField = new kCura.IntegrationPoints.Services.FieldEntry
				{
					FieldIdentifier = mapping.Value,
					DisplayName = mapping.Value,
					IsIdentifier = mapping.Key == testCase.IdentifierFieldName
				},
				DestinationField = new kCura.IntegrationPoints.Services.FieldEntry
				{
					FieldIdentifier = fieldNamesToArtifactIDMapping[mapping.Key].ToString(),
					DisplayName = mapping.Key,
					IsIdentifier = mapping.Key == testCase.IdentifierFieldName
				}
			}).ToList();
			return fieldMapping;
		}

		private async Task AssertJobHistoryIsValidAsync(int integrationPointID, MyFirstProviderTestCase testCase)
		{
			JobHistory jobHistory = await JobHistoryTestHelper.GetCompletedJobHistoryAsync(_objectManager, integrationPointID, testCase.MaximumExecutionTime).ConfigureAwait(false);

			jobHistory.JobStatus.Name.Should().Be(testCase.ExpectedStatus.Name);
			jobHistory.ItemsTransferred.Should().Be(testCase.ExpectedItemsTransferred);
			jobHistory.TotalItems.Should().Be(testCase.ExpectedTotalItems);
		}

		private async Task AssertDocumentsAreImportedAsync(MyFirstProviderTestCase testCase)
		{
			IDictionary<string, string> controlNumberToExtractedTextDictionary = await DocumentsTestHelper
				.GetExtractedTextForDocumentsAsync(
					_objectManager,
					testCase.ExpectedDocumentsIdentifiers)
				.ConfigureAwait(false);

			foreach (string documentIdentifier in testCase.ExpectedDocumentsIdentifiers)
			{
				controlNumberToExtractedTextDictionary.Should().ContainKey(documentIdentifier);
				string expectedExtractedText = testCase.ExpectedDocumentsIdentifiersToExtractedTextMapping[documentIdentifier];
				controlNumberToExtractedTextDictionary[documentIdentifier].Should().Be(expectedExtractedText);
			}
		}

		private Task InitializeWorkspaceSpecificIdentifiersAsync()
		{
			Task[] initializeTasks =
			{
				InitializeMyFirstProviderArtifactIDAsync(),
				InitializeRelativityDestinationProviderArtifactIDAsync(),
				InitializeImportIntegrationPointTypeArtifatIDAsync(),
				InitializeAppendOverlayChoiceArtifactIDAsync(),
				InitializeRootFolderArtifactIDAsync()
			};
			return Task.WhenAll(initializeTasks);
		}

		private async Task InitializeMyFirstProviderArtifactIDAsync()
		{
			List<SourceProvider> sourceProviders = await _objectManager.QueryAsync<SourceProvider>(new QueryRequest()).ConfigureAwait(false);
			SourceProvider myFirstProvider = sourceProviders.Single(x => x.Name == CustomProvidersConstants.MY_FIRST_PROVIDER_SOURCE_PROVIDER_NAME);
			_myFirstProviderArtifactID = myFirstProvider.ArtifactId;
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

		private async Task CreateWorkspaceWithMyFirstProviderAsync()
		{
			string workspaceName = nameof(MyFirstProviderTests);
			_workspaceID = await Workspace.CreateWorkspaceAsync(workspaceName, _WORKSPACE_TEMPLATE_WITHOUT_RIP).ConfigureAwait(false);

			Task importingMyFirstProviderToLibraryTask = ImportMyFirstProviderToLibraryAsync();
			ApplicationManager.InstallApplicationFromLibrary(_workspaceID, CoreConstants.IntegrationPoints.APPLICATION_GUID_STRING);
			await importingMyFirstProviderToLibraryTask.ConfigureAwait(false);
			ApplicationManager.InstallApplicationFromLibrary(_workspaceID, CustomProvidersConstants.MY_FIRST_PROVIDER_GUID);
		}

		private Task ImportMyFirstProviderToLibraryAsync()
		{
			string applicationFilePath = SharedVariables.MyFirstProviderRapFilePath;
			return ApplicationManager.ImportApplicationToLibraryAsync(CustomProvidersConstants.MY_FIRST_PROVIDER_APPLICATION_NAME, applicationFilePath);
		}
	}
}

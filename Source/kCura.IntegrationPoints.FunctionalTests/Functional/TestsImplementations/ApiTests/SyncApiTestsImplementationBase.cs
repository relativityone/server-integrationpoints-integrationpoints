using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Tests.Functional.DataModels;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Framework.Api.Kepler;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Models;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsImplementations.ApiTests
{
    internal class SyncApiTestsImplementationBase
    {
        private Workspace _sourceWorkspace;
        private ICommonIntegrationPointDataService _sourceWorkspaceDataService;
        private const string _SAVED_SEARCH_NAME = "All Documents";
        private readonly IKeplerServiceFactory _serviceFactory;
        private readonly IRipApi _ripApi;
        private readonly IList<Workspace> _destinationWorkspaces = new List<Workspace>();

        protected readonly ITestsImplementationTestFixture TestsImplementationTestFixture;

        protected SyncApiTestsImplementationBase(ITestsImplementationTestFixture testsImplementationTestFixture)
        {
            TestsImplementationTestFixture = testsImplementationTestFixture;
            _serviceFactory = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory;
            _ripApi = new RipApi(_serviceFactory);
        }

        protected void OneTimeSetupExecution(Action importAction)
        {
            _sourceWorkspace = TestsImplementationTestFixture.Workspace;
            importAction();

            CreateSavedSearch(TestsImplementationTestFixture.Workspace.ArtifactID);

            _sourceWorkspaceDataService = new CommonIntegrationPointDataService(_serviceFactory, _sourceWorkspace.ArtifactID);
        }

        public void OneTimeTeardown()
        {
            foreach (var workspace in _destinationWorkspaces)
            {
                RelativityFacade.Instance.DeleteWorkspace(workspace);
            }
        }

        public async Task RunIntegrationPoint()
        {
            // Arrange
            Workspace destinationWorkspace = RelativityFacade.Instance.CreateWorkspace($"SYNC - {Guid.NewGuid()}", TestsImplementationTestFixture.Workspace.Name);
            _destinationWorkspaces.Add(destinationWorkspace);

            ICommonIntegrationPointDataService destinationWorkspaceDataService = new CommonIntegrationPointDataService(_serviceFactory, destinationWorkspace.ArtifactID);

            string integrationPointName = $"{nameof(RunIntegrationPoint)} - {Guid.NewGuid()}";

            IntegrationPointModel integrationPoint = await PrepareIntegrationPointModel(
                  integrationPointName,
                  ImportOverwriteModeEnum.AppendOnly,
                  destinationWorkspaceDataService)
                .ConfigureAwait(false);

            // Act
            await _ripApi.CreateIntegrationPointAsync(integrationPoint, _sourceWorkspace.ArtifactID).ConfigureAwait(false);

            int jobHistoryId = await _ripApi.RunIntegrationPointAsync(integrationPoint, _sourceWorkspace.ArtifactID).ConfigureAwait(false);

            await _ripApi.WaitForJobToFinishAsync(jobHistoryId, _sourceWorkspace.ArtifactID, expectedStatus: JobStatusChoices.JobHistoryCompleted.Name).ConfigureAwait(false);

            // Assert
            List<RelativityObject> sourceWorkspaceAlldocs = await GetDocumentsFromWorkspace(_sourceWorkspace.ArtifactID).ConfigureAwait(false);
            List<RelativityObject> destinationWorkspaceAllDocs = await GetDocumentsFromWorkspace(destinationWorkspace.ArtifactID).ConfigureAwait(false);

            (int transferredItems, int itemsWithErrors, int itemsRead) = await GetTransferredItemsFromJobHistory(jobHistoryId).ConfigureAwait(false);

            itemsWithErrors.Should().Be(0);
            transferredItems.Should().Be(destinationWorkspaceAllDocs.Count);
            itemsRead.Should().Be(destinationWorkspaceAllDocs.Count);

            destinationWorkspaceAllDocs.Should().HaveSameCount(sourceWorkspaceAlldocs);

            IntegrationPointTestModel expectedIntegrationPoint = await _ripApi.GetIntegrationPointAsync(integrationPoint.ArtifactId, _sourceWorkspace.ArtifactID).ConfigureAwait(false);
            expectedIntegrationPoint.HasErrors.Should().BeFalse();
            expectedIntegrationPoint.LastRuntimeUTC.Should().NotBeNull();
        }

        protected async Task RunAndRetryIntegrationPointExecution(Action<Workspace> importAction)
        {
            // 0. Arrange test
            Workspace destinationWorkspace = RelativityFacade.Instance.CreateWorkspace($"SYNC - {Guid.NewGuid()}", TestsImplementationTestFixture.Workspace.Name);
            _destinationWorkspaces.Add(destinationWorkspace);

            importAction(destinationWorkspace);

            ICommonIntegrationPointDataService destinationWorkspaceDataService = new CommonIntegrationPointDataService(_serviceFactory, destinationWorkspace.ArtifactID);

            string integrationPointName = $"{nameof(RunAndRetryIntegrationPointExecution)} - {Guid.NewGuid()}";

            // 1. Job first run:

            // Arrange
            List<RelativityObject> sourceWorkspaceAlldocs = await GetDocumentsFromWorkspace(_sourceWorkspace.ArtifactID).ConfigureAwait(false);
            List<RelativityObject> destinationWorkspaceAllDocs = await GetDocumentsFromWorkspace(destinationWorkspace.ArtifactID).ConfigureAwait(false);

            IntegrationPointModel integrationPoint = await PrepareIntegrationPointModel(integrationPointName, ImportOverwriteModeEnum.OverlayOnly, destinationWorkspaceDataService)
                .ConfigureAwait(false);

            await _ripApi.CreateIntegrationPointAsync(integrationPoint, _sourceWorkspace.ArtifactID).ConfigureAwait(false);

            int expectedItemErrorsToRetry = sourceWorkspaceAlldocs.Count() - destinationWorkspaceAllDocs.Count();

            // Act
            int jobHistoryId = await _ripApi.RunIntegrationPointAsync(integrationPoint, _sourceWorkspace.ArtifactID).ConfigureAwait(false);

            // Assert
            await _ripApi.WaitForJobToFinishAsync(jobHistoryId, _sourceWorkspace.ArtifactID, expectedStatus: JobStatusChoices.JobHistoryCompletedWithErrors.Name).ConfigureAwait(false);

            (int runTransferredItems, int runItemsWithErrors, int itemsRead) = await GetTransferredItemsFromJobHistory(jobHistoryId).ConfigureAwait(false);

            runItemsWithErrors.Should().Be(expectedItemErrorsToRetry);

            IntegrationPointTestModel expectedIntegrationPoint = await _ripApi.GetIntegrationPointAsync(integrationPoint.ArtifactId, _sourceWorkspace.ArtifactID).ConfigureAwait(false);
            expectedIntegrationPoint.HasErrors.Should().BeTrue();
            expectedIntegrationPoint.LastRuntimeUTC.Should().NotBeNull();

            DateTime lastRuntimeUTCOnRun = expectedIntegrationPoint.LastRuntimeUTC.Value;

            // 2. Job retry:

            // Act
            int retryJobHistoryId = await _ripApi.RetryIntegrationPointAsync(integrationPoint, _sourceWorkspace.ArtifactID, true);

            // Assert
            await _ripApi.WaitForJobToFinishAsync(retryJobHistoryId, _sourceWorkspace.ArtifactID,
                expectedStatus: JobStatusChoices.JobHistoryCompleted.Name).ConfigureAwait(false);

            (int retryTransferredItems, int retryItemsWithErrors, int itemsReadOnRetry) = await GetTransferredItemsFromJobHistory(retryJobHistoryId).ConfigureAwait(false);

            retryItemsWithErrors.Should().Be(0);
            retryTransferredItems.Should().Be(expectedItemErrorsToRetry);

            expectedIntegrationPoint = await _ripApi.GetIntegrationPointAsync(integrationPoint.ArtifactId, _sourceWorkspace.ArtifactID).ConfigureAwait(false);
            expectedIntegrationPoint.HasErrors.Should().BeFalse();
            expectedIntegrationPoint.LastRuntimeUTC.Should().BeAfter(lastRuntimeUTCOnRun);
        }

        protected int CreateSavedSearch(int workspaceId)
        {
            KeywordSearch keywordSearch = new KeywordSearch { Name = _SAVED_SEARCH_NAME };
            return RelativityFacade.Instance.Resolve<IKeywordSearchService>().Require(workspaceId, keywordSearch)
                .ArtifactID;
        }

        private async Task<IntegrationPointModel> PrepareIntegrationPointModel(string integrationPointName,
            ImportOverwriteModeEnum overwriteMode, ICommonIntegrationPointDataService destinationWorkspaceDataService)
        {
            int savedSearchId = await _sourceWorkspaceDataService.GetSavedSearchArtifactIdAsync(_SAVED_SEARCH_NAME).ConfigureAwait(false);
            int destinationFolderId = await destinationWorkspaceDataService.GetRootFolderArtifactIdAsync().ConfigureAwait(false);
            int destinationWorkspaceId = destinationWorkspaceDataService.WorkspaceId;

            return new IntegrationPointModel
            {
                SourceConfiguration = GetSourceConfiguartion(savedSearchId),
                DestinationConfiguration = GetDestinationConfiguration(destinationWorkspaceDataService.WorkspaceId, destinationFolderId),
                Name = integrationPointName,
                FieldMappings = await _sourceWorkspaceDataService.GetIdentifierMappingAsync(destinationWorkspaceId).ConfigureAwait(false),
                DestinationProvider = await _sourceWorkspaceDataService.GetDestinationProviderIdAsync(kCura.IntegrationPoints.Core.Constants.IntegrationPoints.DestinationProviders.RELATIVITY).ConfigureAwait(false),
                SourceProvider = await _sourceWorkspaceDataService.GetSourceProviderIdAsync(kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.RELATIVITY).ConfigureAwait(false),
                Type = await _sourceWorkspaceDataService.GetIntegrationPointTypeByAsync(kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportName).ConfigureAwait(false),
                OverwriteFieldsChoiceId = await _sourceWorkspaceDataService.GetOverwriteFieldsChoiceIdAsync(overwriteMode).ConfigureAwait(false),
                EmailNotificationRecipients = string.Empty,
                ScheduleRule = new ScheduleModel(),
                LogErrors = true
            };
        }

        private RelativityProviderSourceConfiguration GetSourceConfiguartion(int savedSearchId)
        {
            return new RelativityProviderSourceConfiguration
            {
                TypeOfExport = (int)SourceConfiguration.ExportType.SavedSearch,
                SavedSearchArtifactId = savedSearchId,
                SourceWorkspaceArtifactId = _sourceWorkspace.ArtifactID,
                UseDynamicFolderPath = false
            };
        }

        protected virtual DestinationConfiguration GetDestinationConfiguration(int destinationWorkspaceId, int destinationFolderId)
        {
            return new DestinationConfiguration
            {
                CaseArtifactId = destinationWorkspaceId,
                FieldOverlayBehavior = RelativityProviderValidationMessages.FIELD_MAP_FIELD_OVERLAY_BEHAVIOR_DEFAULT,
                ImportNativeFile = false,
                ArtifactTypeId = (int)ArtifactType.Document,
                DestinationFolderArtifactId = destinationFolderId,
                FolderPathSourceField = 0,
                UseFolderPathInformation = false,
                ImageImport = false
            };
        }

        private async Task<List<RelativityObject>> GetDocumentsFromWorkspace(int workspaceId)
        {
            using (var objectManager = _serviceFactory
                       .GetServiceProxy<IObjectManager>())
            {
                QueryRequest request = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.Document },
                    Fields = new FieldRef[] { new FieldRef { Name = "*" } }
                };
                QueryResult result = await objectManager.QueryAsync(workspaceId, request, 0, int.MaxValue).ConfigureAwait(false);

                return result.Objects.ToList();
            }
        }

        private async Task<(int TransferredItems, int ItemsWithErrors, int ItemsRead)> GetTransferredItemsFromJobHistory(int jobHistoryId)
        {
            RelativityObject jobHistoryDetails = await GetJobHistoryById(jobHistoryId);
            int transferredItems = (int)jobHistoryDetails.FieldValues.Where(f => f.Field.Name == JobHistoryFields.ItemsTransferred).FirstOrDefault().Value;
            int itemsWithError = (int)jobHistoryDetails.FieldValues.Where(f => f.Field.Name == JobHistoryFields.ItemsWithErrors).FirstOrDefault().Value;
            int itemsRead = (int)jobHistoryDetails.FieldValues.Where(f => f.Field.Name == JobHistoryFields.ItemsRead).FirstOrDefault().Value;

            return (transferredItems, itemsWithError, itemsRead);
        }

        private async Task<RelativityObject> GetJobHistoryById(int jobHistoryId)
        {
            using (IObjectManager objectManager = _serviceFactory.GetServiceProxy<IObjectManager>())
            {
                QueryRequest request = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef { Guid = ObjectTypeGuids.JobHistoryGuid },
                    Fields = new FieldRef[]
                    {
                        new FieldRef { Name = JobHistoryFields.ItemsTransferred },
                        new FieldRef { Name = JobHistoryFields.ItemsWithErrors },
                        new FieldRef { Name = JobHistoryFields.ItemsRead }
                    },
                    Condition = $"'ArtifactId' == '{jobHistoryId}'"
                };
                QueryResult result = await objectManager.QueryAsync(_sourceWorkspace.ArtifactID, request, 0, int.MaxValue).ConfigureAwait(false);
                return result.Objects.FirstOrDefault();
            }
        }
    }
}

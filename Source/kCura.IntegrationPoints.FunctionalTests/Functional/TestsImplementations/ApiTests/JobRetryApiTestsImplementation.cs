using Relativity.IntegrationPoints.Tests.Functional.Helpers.API;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Framework.Models;
using KeywordSearch = Relativity.Testing.Framework.Models.KeywordSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.LoadFiles;
using Relativity.Testing.Framework.Api.Services;
using Relativity.IntegrationPoints.Services;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using kCura.IntegrationPoints.Data;
using FluentAssertions;
using static kCura.IntegrationPoints.Core.Constants.IntegrationPoints;
using Relativity.Testing.Framework.Api.Kepler;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsImplementations.ApiTests
{
    internal class JobRetryApiTestsImplementation
    {       
        private int _transferredItems;
        private int _itemsWithError;
        private Workspace _destinationWorkspace;
        private Workspace _sourceWorkspace;

        private ICommonIntegrationPointDataService _sourceWorkspaceDataService;
        private ICommonIntegrationPointDataService _destinationWorkspaceDataService;

        private const string SAVED_SEARCH_NAME = "AllDocuments";
        private const string JOB_RETRY_DESTINATION_WORKSPACE_NAME = "RIP Job Retry Test";
        private const int _destinationWorkspaceDocumentLimit = 4;
        private readonly ITestsImplementationTestFixture _testsImplementationTestFixture;
        private readonly IKeplerServiceFactory _serviceFactory;         
        private readonly IRipApi _ripApi;        

        public JobRetryApiTestsImplementation(ITestsImplementationTestFixture testsImplementationTestFixture)
        {
            _testsImplementationTestFixture = testsImplementationTestFixture;
            _serviceFactory = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory;           
            _ripApi = new RipApi(_serviceFactory);
        }

        public void OnSetUpFixture()
        {
            _sourceWorkspace = _testsImplementationTestFixture.Workspace;           
            RelativityFacade.Instance.ImportDocumentsFromCsv(_testsImplementationTestFixture.Workspace, LoadFilesGenerator.GetOrCreateNativesLoadFile(), overwriteMode: DocumentOverwriteMode.AppendOverlay);
            _destinationWorkspace = RelativityFacade.Instance.CreateWorkspace(JOB_RETRY_DESTINATION_WORKSPACE_NAME, _testsImplementationTestFixture.Workspace.Name);
            CreateSavedSearch(_testsImplementationTestFixture.Workspace.ArtifactID);
            RelativityFacade.Instance.ImportDocumentsFromCsv(_destinationWorkspace, LoadFilesGenerator.CreateNativesLoadFileWithLimitedItems(_destinationWorkspaceDocumentLimit), overwriteMode: DocumentOverwriteMode.AppendOverlay);
            _sourceWorkspaceDataService = new CommonIntegrationPointDataService(_serviceFactory, _sourceWorkspace.ArtifactID);
            _destinationWorkspaceDataService = new CommonIntegrationPointDataService(_serviceFactory, _destinationWorkspace.ArtifactID);
        }

        public void OnTearDownFixture()
        {
            RelativityFacade.Instance.DeleteWorkspace(_destinationWorkspace);
        }

        public async Task RunAndRetryIntegrationPoint()
        {
            //1. Job first run:

            //Arrange
            List<RelativityObject> sourceWorkspaceAlldocs = await GetDocumentsFromWorkspace(_sourceWorkspace.ArtifactID).ConfigureAwait(false);
            List<RelativityObject> destinationWorkspaceAllDocs = await GetDocumentsFromWorkspace(_destinationWorkspace.ArtifactID).ConfigureAwait(false);          

            IntegrationPointModel integrationPoint = await PrepareIntegrationPointModel().ConfigureAwait(false);
            await _ripApi.CreateIntegrationPointAsync(integrationPoint, _sourceWorkspace.ArtifactID).ConfigureAwait(false);

            int expectedItemErrorsToRetry = sourceWorkspaceAlldocs.Count() - destinationWorkspaceAllDocs.Count();
            string expectedStatus = JobStatusChoices.JobHistoryCompletedWithErrors.Name;           

            //Act
            int jobHistoryId = await _ripApi.RunIntegrationPointAsync(integrationPoint, _sourceWorkspace.ArtifactID).ConfigureAwait(false);

            //Assert             
            Func<Task> endRun = async () => { await _ripApi.WaitForJobToFinishAsync(jobHistoryId, _sourceWorkspace.ArtifactID, checkDelayInMs: 500, expectedStatus: expectedStatus).ConfigureAwait(false); };
            endRun.ShouldNotThrow();
            await GetJobHistoryDetails(jobHistoryId).ConfigureAwait(false);
            _itemsWithError.Should().Be(expectedItemErrorsToRetry);

            //2. Job retry:

            //Arrange
            integrationPoint.OverwriteFieldsChoiceId = await _sourceWorkspaceDataService.GetOverwriteFieldsChoiceIdAsync(OverwriteFieldsChoices.IntegrationPointAppendOverlay.Name).ConfigureAwait(false);
            expectedStatus = JobStatusChoices.JobHistoryCompleted.Name;

            //Act
            int retryJobHistoryId = await _ripApi.RetryIntegrationPointAsync(integrationPoint, _sourceWorkspace.ArtifactID);

            //Assert
            Func<Task> endRetry = async () => { await _ripApi.WaitForJobToFinishAsync(retryJobHistoryId, _sourceWorkspace.ArtifactID, checkDelayInMs: 500, expectedStatus: expectedStatus).ConfigureAwait(false); };
            endRetry.ShouldNotThrow();
            await GetJobHistoryDetails(retryJobHistoryId).ConfigureAwait(false);
            _transferredItems.Should().Be(expectedItemErrorsToRetry);
        }

        private void CreateSavedSearch(int workspaceId)
        {
            KeywordSearch keywordSearch = new KeywordSearch { Name = SAVED_SEARCH_NAME };
            RelativityFacade.Instance.Resolve<IKeywordSearchService>().Require(workspaceId, keywordSearch);
        }

        private async Task<IntegrationPointModel> PrepareIntegrationPointModel()
        {
            int savedSearchId = await _sourceWorkspaceDataService.GetSavedSearchArtifactIdAsync(SAVED_SEARCH_NAME).ConfigureAwait(false);
            int destinationFolderId = await _destinationWorkspaceDataService.GetRootFolderArtifactIdAsync().ConfigureAwait(false);

            return new IntegrationPointModel
            {
                SourceConfiguration = GetSourceConfig(savedSearchId),
                DestinationConfiguration = GetDestinationConfiguration(destinationFolderId),
                Name = $"{JOB_RETRY_DESTINATION_WORKSPACE_NAME} {_destinationWorkspace.ArtifactID}",
                FieldMappings = await _sourceWorkspaceDataService.GetIdentifierMappingAsync(_destinationWorkspace.ArtifactID).ConfigureAwait(false),
                DestinationProvider = await _sourceWorkspaceDataService.GetDestinationProviderIdAsync(DestinationProviders.RELATIVITY).ConfigureAwait(false),
                SourceProvider = await _sourceWorkspaceDataService.GetSourceProviderIdAsync(SourceProviders.RELATIVITY).ConfigureAwait(false),
                Type = await _sourceWorkspaceDataService.GetIntegrationPointTypeByAsync(IntegrationPointTypes.ExportName).ConfigureAwait(false),
                OverwriteFieldsChoiceId = await _sourceWorkspaceDataService.GetOverwriteFieldsChoiceIdAsync(OverwriteFieldsChoices.IntegrationPointOverlayOnly.Name).ConfigureAwait(false),
                EmailNotificationRecipients = string.Empty,
                ScheduleRule = new ScheduleModel(),
                LogErrors = true
            };
        }

        private RelativityProviderSourceConfiguration GetSourceConfig(int savedSearchId)
        {
            return new RelativityProviderSourceConfiguration
            {
                TypeOfExport = (int)SourceConfiguration.ExportType.SavedSearch,
                SavedSearchArtifactId = savedSearchId,
                SourceWorkspaceArtifactId = _sourceWorkspace.ArtifactID,
                UseDynamicFolderPath = false
            };
        }

        private RelativityProviderDestinationConfiguration GetDestinationConfiguration(int folderId)
        {
            return new RelativityProviderDestinationConfiguration
            {
                CaseArtifactId = _destinationWorkspace.ArtifactID,
                FieldOverlayBehavior = RelativityProviderValidationMessages.FIELD_MAP_FIELD_OVERLAY_BEHAVIOR_DEFAULT,
                ImportNativeFile = false,
                ArtifactTypeID = (int)ArtifactType.Document,
                DestinationFolderArtifactId = folderId,
                FolderPathSourceField = 0,
                UseFolderPathInformation = false
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

        private async Task GetJobHistoryDetails(int jobHistoryId)
        {
            RelativityObject jobHistoryDetails = await GetJobHistoryByObjectManager(jobHistoryId);
            _transferredItems = (int)jobHistoryDetails.FieldValues.Where(f => f.Field.Name == JobHistoryFields.ItemsTransferred).FirstOrDefault().Value;
            _itemsWithError = (int)jobHistoryDetails.FieldValues.Where(f => f.Field.Name == JobHistoryFields.ItemsWithErrors).FirstOrDefault().Value;
        }

        private async Task<RelativityObject> GetJobHistoryByObjectManager(int jobHistoryId)
        {
            using (IObjectManager objectManager = _serviceFactory.GetServiceProxy<IObjectManager>())
            {
                QueryRequest request = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef { Guid = ObjectTypeGuids.JobHistoryGuid },
                    Fields = new FieldRef[]
                    {
                        new FieldRef { Name = JobHistoryFields.ItemsTransferred },
                        new FieldRef { Name = JobHistoryFields.ItemsWithErrors }
                    },
                    Condition = $"'ArtifactId' == '{jobHistoryId}'"
                };
                QueryResult result = await objectManager.QueryAsync(_sourceWorkspace.ArtifactID, request, 0, int.MaxValue).ConfigureAwait(false);
                return result.Objects.FirstOrDefault();
            }
        }

    }
}

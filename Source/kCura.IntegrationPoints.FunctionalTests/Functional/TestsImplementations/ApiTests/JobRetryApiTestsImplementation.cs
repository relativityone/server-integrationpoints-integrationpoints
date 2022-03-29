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

namespace Relativity.IntegrationPoints.Tests.Functional.TestsImplementations.ApiTests
{
    internal class JobRetryApiTestsImplementation
    {
        private const string SAVED_SEARCH_NAME = "AllDocuments";
        private const string JOB_RETRY_WORKSPACE_NAME = "RIP Job Retry Test";
        private readonly ITestsImplementationTestFixture _testsImplementationTestFixture;
        private readonly IRipApi ripApi = new RipApi(RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory);       

        private ICommonIntegrationPointDataService dataService;

        private int initialSourceWorkspaceDocsCount;
        private int initialDestinationWorkspaceDocsCount;
        private Workspace destinationWorkspace;
        public Workspace SourceWorkspace;

        public JobRetryApiTestsImplementation(ITestsImplementationTestFixture testsImplementationTestFixture)
        {
            _testsImplementationTestFixture = testsImplementationTestFixture;
        }

        public void OnSetUpFixture()
        {
            SourceWorkspace = _testsImplementationTestFixture.Workspace;
            RelativityFacade.Instance.ImportDocumentsFromCsv(_testsImplementationTestFixture.Workspace, LoadFilesGenerator.GetOrCreateNativesLoadFile(), overwriteMode: DocumentOverwriteMode.AppendOverlay);
            destinationWorkspace = RelativityFacade.Instance.CreateWorkspace(JOB_RETRY_WORKSPACE_NAME, _testsImplementationTestFixture.Workspace.Name);
            CreateSavedSearch(_testsImplementationTestFixture.Workspace.ArtifactID);

            RelativityFacade.Instance.ImportDocumentsFromCsv(destinationWorkspace, LoadFilesGenerator.CreateNativesLoadFileWithLimitedItems(4), overwriteMode: DocumentOverwriteMode.AppendOverlay);

            initialSourceWorkspaceDocsCount = GetDocumentsFromWorkspace(SourceWorkspace.ArtifactID).Count();
            initialDestinationWorkspaceDocsCount = GetDocumentsFromWorkspace(destinationWorkspace.ArtifactID).Count();

            dataService = new CommonIntegrationPointDataService(RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory, SourceWorkspace.ArtifactID);
        }

        public void OnTearDownFixture()
        {
            RelativityFacade.Instance.DeleteWorkspace(destinationWorkspace);
        }

        public async Task RunAndRetryIntegrationPoint()
        {
            //1. Job first run:

            //Arrange
            IntegrationPointModel integrationPoint = await PrepareIntegrationPointModel().ConfigureAwait(false);
            await ripApi.CreateIntegrationPointAsync(integrationPoint, SourceWorkspace.ArtifactID);
            int expectedItemErrorsToRetry = initialSourceWorkspaceDocsCount - initialDestinationWorkspaceDocsCount;
            string expectedStatus = JobStatusChoices.JobHistoryCompletedWithErrors.Name;
            int transferredItems;
            int itemsWithErrors;

            //Act
            int jobHistoryId = await ripApi.RunIntegrationPointAsync(integrationPoint, SourceWorkspace.ArtifactID);

            //Assert             
            Func<Task> endRun = async () => { await ripApi.WaitForJobToFinishAsync(jobHistoryId, SourceWorkspace.ArtifactID, checkDelayInMs: 500, expectedStatus: expectedStatus).ConfigureAwait(false); };
            endRun.ShouldNotThrow();
            GetJobHistoryDetails(jobHistoryId, out transferredItems, out itemsWithErrors);
            itemsWithErrors.Should().Be(expectedItemErrorsToRetry);

            //2. Job retry:

            //Arrange
            integrationPoint.OverwriteFieldsChoiceId = await dataService.GetOverwriteFieldsChoiceIdAsync(OverwriteFieldsChoices.IntegrationPointAppendOverlay.Name).ConfigureAwait(false);
            expectedStatus = JobStatusChoices.JobHistoryCompleted.Name;

            //Act
            int retryJobHistoryId = await ripApi.RetryIntegrationPointAsync(integrationPoint, SourceWorkspace.ArtifactID);

            //Assert
            Func<Task> endRetry = async () => { await ripApi.WaitForJobToFinishAsync(retryJobHistoryId, SourceWorkspace.ArtifactID, checkDelayInMs: 500, expectedStatus: expectedStatus).ConfigureAwait(false); };
            endRetry.ShouldNotThrow();
            GetJobHistoryDetails(retryJobHistoryId, out transferredItems, out itemsWithErrors);
            transferredItems.Should().Be(expectedItemErrorsToRetry);
        }

        private void CreateSavedSearch(int workspaceId)
        {
            KeywordSearch keywordSearch = new KeywordSearch { Name = SAVED_SEARCH_NAME };
            RelativityFacade.Instance.Resolve<IKeywordSearchService>().Require(workspaceId, keywordSearch);
        }

        private async Task<IntegrationPointModel> PrepareIntegrationPointModel()
        {
            int savedSearchId = await dataService.GetSavedSearchArtifactIdAsync(SAVED_SEARCH_NAME).ConfigureAwait(false);
            int destinationFolderId = await dataService.GetRootFolderArtifactIdAsync(destinationWorkspace.ArtifactID).ConfigureAwait(false);

            return new IntegrationPointModel
            {
                SourceConfiguration = GetSourceConfig(savedSearchId),
                DestinationConfiguration = GetDestinationConfiguration(destinationFolderId),
                Name = $"{JOB_RETRY_WORKSPACE_NAME} {destinationWorkspace.ArtifactID}",
                FieldMappings = await dataService.GetIdentifierMappingAsync(SourceWorkspace.ArtifactID, destinationWorkspace.ArtifactID).ConfigureAwait(false),
                DestinationProvider = await dataService.GetDestinationProviderIdAsync(DestinationProviders.RELATIVITY).ConfigureAwait(false),
                SourceProvider = await dataService.GetSourceProviderIdAsync(SourceProviders.RELATIVITY).ConfigureAwait(false),
                Type = await dataService.GetIntegrationPointTypeByAsync(IntegrationPointTypes.ExportName).ConfigureAwait(false),
                OverwriteFieldsChoiceId = await dataService.GetOverwriteFieldsChoiceIdAsync(OverwriteFieldsChoices.IntegrationPointOverlayOnly.Name).ConfigureAwait(false),
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
                SourceWorkspaceArtifactId = SourceWorkspace.ArtifactID,
                UseDynamicFolderPath = false
            };
        }

        private RelativityProviderDestinationConfiguration GetDestinationConfiguration(int folderId)
        {
            return new RelativityProviderDestinationConfiguration
            {
                CaseArtifactId = destinationWorkspace.ArtifactID,
                FieldOverlayBehavior = RelativityProviderValidationMessages.FIELD_MAP_FIELD_OVERLAY_BEHAVIOR_DEFAULT,
                ImportNativeFile = false,
                ArtifactTypeID = (int)ArtifactType.Document,
                DestinationFolderArtifactId = folderId,
                FolderPathSourceField = 0,
                UseFolderPathInformation = false
            };
        }      

        private List<RelativityObject> GetDocumentsFromWorkspace(int workspaceId)
        {
            using (IObjectManager objectManager = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory.GetServiceProxy<IObjectManager>())
            {
                QueryRequest request = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.Document },
                    Fields = new FieldRef[] { new FieldRef { Name = "*" } }
                };
                return objectManager.QueryAsync(workspaceId, request, 0, int.MaxValue).GetAwaiter().GetResult().Objects.ToList();
            }
        }

        private void GetJobHistoryDetails(int jobHistoryId, out int transferredItems, out int itemsWithError)
        {
            RelativityObject jobHistoryDetails = GetJobHistoryByObjectManager(jobHistoryId);
            transferredItems = (int)jobHistoryDetails.FieldValues.Where(f => f.Field.Name == JobHistoryFields.ItemsTransferred).FirstOrDefault().Value;
            itemsWithError = (int)jobHistoryDetails.FieldValues.Where(f => f.Field.Name == JobHistoryFields.ItemsWithErrors).FirstOrDefault().Value;
        }

        private RelativityObject GetJobHistoryByObjectManager(int jobHistoryId)
        {
            using (IObjectManager objectManager = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory.GetServiceProxy<IObjectManager>())
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

                return objectManager.QueryAsync(SourceWorkspace.ArtifactID, request, 0, int.MaxValue)
                    .GetAwaiter().GetResult().Objects.ToList().FirstOrDefault();
            }
        }

    }
}

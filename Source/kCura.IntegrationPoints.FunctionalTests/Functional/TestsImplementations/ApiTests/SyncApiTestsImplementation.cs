﻿using Relativity.IntegrationPoints.Tests.Functional.Helpers.API;
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
    internal class SyncApiTestsImplementation
    {
        private Workspace _sourceWorkspace;

        private ICommonIntegrationPointDataService _sourceWorkspaceDataService;

        private const string _SAVED_SEARCH_NAME = "All Documents";

        private readonly ITestsImplementationTestFixture _testsImplementationTestFixture;
        private readonly IKeplerServiceFactory _serviceFactory;
        private readonly IRipApi _ripApi;

        private readonly IList<Workspace> _destinationWorkspaces = new List<Workspace>();

        public SyncApiTestsImplementation(ITestsImplementationTestFixture testsImplementationTestFixture)
        {
            _testsImplementationTestFixture = testsImplementationTestFixture;
            _serviceFactory = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory;
            _ripApi = new RipApi(_serviceFactory);
        }

        public void OneTimeSetup()
        {
            _sourceWorkspace = _testsImplementationTestFixture.Workspace;
            RelativityFacade.Instance.ImportDocumentsFromCsv(_testsImplementationTestFixture.Workspace,
                LoadFilesGenerator.GetOrCreateNativesLoadFile(), overwriteMode: DocumentOverwriteMode.AppendOverlay);

            CreateSavedSearch(_testsImplementationTestFixture.Workspace.ArtifactID);

            _sourceWorkspaceDataService = new CommonIntegrationPointDataService(_serviceFactory, _sourceWorkspace.ArtifactID);
        }

        public void OneTimeTeardown()
        {
            foreach(var workspace in _destinationWorkspaces)
            {
                RelativityFacade.Instance.DeleteWorkspace(workspace);
            }
        }

        public async Task RunIntegrationPoint()
        {
            // Arrange
            Workspace destinationWorkspace = RelativityFacade.Instance.CreateWorkspace($"SYNC - {Guid.NewGuid()}", _testsImplementationTestFixture.Workspace.Name);
            _destinationWorkspaces.Add(destinationWorkspace);

            ICommonIntegrationPointDataService destinationWorkspaceDataService = new CommonIntegrationPointDataService(_serviceFactory, destinationWorkspace.ArtifactID);

            string integrationPointName = $"{nameof(RunIntegrationPoint)} - {Guid.NewGuid()}";
            
            IntegrationPointModel integrationPoint = await PrepareIntegrationPointModel(integrationPointName, destinationWorkspaceDataService).ConfigureAwait(false);

            List<RelativityObject> sourceWorkspaceAlldocs = await GetDocumentsFromWorkspace(_sourceWorkspace.ArtifactID).ConfigureAwait(false);

            // Act
            await _ripApi.CreateIntegrationPointAsync(integrationPoint, _sourceWorkspace.ArtifactID).ConfigureAwait(false);

            int jobHistoryId = await _ripApi.RunIntegrationPointAsync(integrationPoint, _sourceWorkspace.ArtifactID).ConfigureAwait(false);

            await _ripApi.WaitForJobToFinishAsync(jobHistoryId, _sourceWorkspace.ArtifactID,
                    expectedStatus: JobStatusChoices.JobHistoryCompleted.Name).ConfigureAwait(false);

            // Assert
            List<RelativityObject> destinationWorkspaceAllDocs = await GetDocumentsFromWorkspace(destinationWorkspace.ArtifactID).ConfigureAwait(false);

            (int TransferredItems, int ItemsWithErrors) = await GetTransferredItemsFromJobHistory(jobHistoryId).ConfigureAwait(false);

            ItemsWithErrors.Should().Be(0);
            TransferredItems.Should().Be(destinationWorkspaceAllDocs.Count);

            destinationWorkspaceAllDocs.Should().HaveSameCount(sourceWorkspaceAlldocs);
        }

        public async Task RunAndRetryIntegrationPoint()
        {
            //0. Arrange test
            const int destinationWorkspaceInitialImportCount = 4;

            Workspace destinationWorkspace = RelativityFacade.Instance.CreateWorkspace($"SYNC - {Guid.NewGuid()}", _testsImplementationTestFixture.Workspace.Name);
            _destinationWorkspaces.Add(destinationWorkspace);

            RelativityFacade.Instance.ImportDocumentsFromCsv(destinationWorkspace,
                LoadFilesGenerator.CreateNativesLoadFileWithLimitedItems(destinationWorkspaceInitialImportCount),
                overwriteMode: DocumentOverwriteMode.AppendOverlay);

            ICommonIntegrationPointDataService destinationWorkspaceDataService = new CommonIntegrationPointDataService(_serviceFactory, destinationWorkspace.ArtifactID);

            string integrationPointName = $"{nameof(RunAndRetryIntegrationPoint)} - {Guid.NewGuid()}";

            //1. Job first run:

            //Arrange
            List<RelativityObject> sourceWorkspaceAlldocs = await GetDocumentsFromWorkspace(_sourceWorkspace.ArtifactID).ConfigureAwait(false);
            List<RelativityObject> destinationWorkspaceAllDocs = await GetDocumentsFromWorkspace(destinationWorkspace.ArtifactID).ConfigureAwait(false);

            IntegrationPointModel integrationPoint = await PrepareIntegrationPointModel(integrationPointName, destinationWorkspaceDataService).ConfigureAwait(false);
            
            await _ripApi.CreateIntegrationPointAsync(integrationPoint, _sourceWorkspace.ArtifactID).ConfigureAwait(false);

            int expectedItemErrorsToRetry = sourceWorkspaceAlldocs.Count() - destinationWorkspaceAllDocs.Count();

            //Act
            int jobHistoryId = await _ripApi.RunIntegrationPointAsync(integrationPoint, _sourceWorkspace.ArtifactID).ConfigureAwait(false);

            //Assert
            Func<Task> runTask = async () => 
            { 
                await _ripApi.WaitForJobToFinishAsync(jobHistoryId, _sourceWorkspace.ArtifactID,
                    expectedStatus: JobStatusChoices.JobHistoryCompletedWithErrors.Name).ConfigureAwait(false);
            };
            runTask.ShouldNotThrow();
            
            (int RunTransferredItems, int RunItemsWithErrors) = await GetTransferredItemsFromJobHistory(jobHistoryId).ConfigureAwait(false);

            RunItemsWithErrors.Should().Be(expectedItemErrorsToRetry);

            //2. Job retry:

            //Arrange
            integrationPoint.OverwriteFieldsChoiceId = await _sourceWorkspaceDataService.GetOverwriteFieldsChoiceIdAsync(OverwriteFieldsChoices.IntegrationPointAppendOverlay.Name).ConfigureAwait(false);

            //Act
            int retryJobHistoryId = await _ripApi.RetryIntegrationPointAsync(integrationPoint, _sourceWorkspace.ArtifactID);

            //Assert
            Func<Task> retryTask = async () =>
            { 
                await _ripApi.WaitForJobToFinishAsync(retryJobHistoryId, _sourceWorkspace.ArtifactID,
                    expectedStatus: JobStatusChoices.JobHistoryCompleted.Name).ConfigureAwait(false);
            };
            retryTask.ShouldNotThrow();
            
            (int RetryTransferredItems, int RetryItemsWithErrors) = await GetTransferredItemsFromJobHistory(retryJobHistoryId).ConfigureAwait(false);

            RetryItemsWithErrors.Should().Be(0);
            RetryTransferredItems.Should().Be(expectedItemErrorsToRetry);
        }

        private int CreateSavedSearch(int workspaceId)
        {
            KeywordSearch keywordSearch = new KeywordSearch { Name = _SAVED_SEARCH_NAME };
            return RelativityFacade.Instance.Resolve<IKeywordSearchService>().Require(workspaceId, keywordSearch)
                .ArtifactID;
        }

        private async Task<IntegrationPointModel> PrepareIntegrationPointModel(string integrationPointName,
            ICommonIntegrationPointDataService destinationWorkspaceDataService)
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
                DestinationProvider = await _sourceWorkspaceDataService.GetDestinationProviderIdAsync(DestinationProviders.RELATIVITY).ConfigureAwait(false),
                SourceProvider = await _sourceWorkspaceDataService.GetSourceProviderIdAsync(SourceProviders.RELATIVITY).ConfigureAwait(false),
                Type = await _sourceWorkspaceDataService.GetIntegrationPointTypeByAsync(IntegrationPointTypes.ExportName).ConfigureAwait(false),
                OverwriteFieldsChoiceId = await _sourceWorkspaceDataService.GetOverwriteFieldsChoiceIdAsync(OverwriteFieldsChoices.IntegrationPointOverlayOnly.Name).ConfigureAwait(false),
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

        private RelativityProviderDestinationConfiguration GetDestinationConfiguration(int destinationWorkspaceId, int destinationFolderId)
        {
            return new RelativityProviderDestinationConfiguration
            {
                CaseArtifactId = destinationWorkspaceId,
                FieldOverlayBehavior = RelativityProviderValidationMessages.FIELD_MAP_FIELD_OVERLAY_BEHAVIOR_DEFAULT,
                ImportNativeFile = false,
                ArtifactTypeID = (int)ArtifactType.Document,
                DestinationFolderArtifactId = destinationFolderId,
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

        private async Task<(int TransferredItems, int ItemsWithErrors)> GetTransferredItemsFromJobHistory(int jobHistoryId)
        {
            RelativityObject jobHistoryDetails = await GetJobHistoryById(jobHistoryId);
            int transferredItems = (int)jobHistoryDetails.FieldValues.Where(f => f.Field.Name == JobHistoryFields.ItemsTransferred).FirstOrDefault().Value;
            int itemsWithError = (int)jobHistoryDetails.FieldValues.Where(f => f.Field.Name == JobHistoryFields.ItemsWithErrors).FirstOrDefault().Value;

            return (transferredItems, itemsWithError);
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
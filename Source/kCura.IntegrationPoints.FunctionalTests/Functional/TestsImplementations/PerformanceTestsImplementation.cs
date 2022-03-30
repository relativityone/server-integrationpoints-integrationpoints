using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.API;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.LoadFiles;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Models;
using static kCura.IntegrationPoints.Core.Constants.IntegrationPoints;
using KeywordSearch = Relativity.Testing.Framework.Models.KeywordSearch;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsImplementations
{
    internal class PerformanceTestsImplementation
    {
        private const string SavedSearchName = "AllDocuments";
        private readonly ITestsImplementationTestFixture _testsImplementationTestFixture;
        private ICommonIntegrationPointDataService _sourceWorkspaceDataService;
        private ICommonIntegrationPointDataService _destinationWorkspaceDataService;
        private Workspace[] _destinationWorkspaces;
        private List<int> _integrationPoints = new List<int>();

        private readonly IRipApi _ripApi =
            new RipApi(RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory);

        private int _savedSearchId;
        private int _destinationProviderId;
        private int _sourceProviderId;
        private int _integrationPointType;       

        public Workspace SourceWorkspace => _testsImplementationTestFixture.Workspace;

        public PerformanceTestsImplementation(ITestsImplementationTestFixture testsImplementationTestFixture)
        {
            _testsImplementationTestFixture = testsImplementationTestFixture;
        }

        public void OnSetUpFixture(int runCount)
        {
            RelativityFacade.Instance.ImportDocumentsFromCsv(_testsImplementationTestFixture.Workspace,
                LoadFilesGenerator.GetOrCreateNativesLoadFile(), overwriteMode: DocumentOverwriteMode.AppendOverlay);

            _sourceWorkspaceDataService = new CommonIntegrationPointDataService(RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory, _testsImplementationTestFixture.Workspace.ArtifactID);
            
            CreateSavedSearch(_testsImplementationTestFixture.Workspace.ArtifactID);

            GetIntegrationPointsConstantsAsync().GetAwaiter().GetResult();

            _destinationWorkspaces = Enumerable.Range(0, runCount)
                .Select(i =>
                    RelativityFacade.Instance.CreateWorkspace(
                        string.Format(PerformanceTestsConstants.PERFORMANCE_TEST_WORKSPACE_NAME_FORMAT, i), SourceWorkspace.Name))
                .ToArray();
        }

        private void CreateSavedSearch(int workspaceId)
        {
            KeywordSearch keywordSearch = new KeywordSearch
            {
                Name = SavedSearchName
            };

            RelativityFacade.Instance.Resolve<IKeywordSearchService>()
                .Require(workspaceId, keywordSearch);
        }

        public void OnTearDownFixture()
        {
            foreach (var destinationWorkspace in _destinationWorkspaces)
            {
                RelativityFacade.Instance.DeleteWorkspace(destinationWorkspace);
            }

            if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Passed)
            {
                DeleteIntegrationPoints(_integrationPoints, SourceWorkspace.ArtifactID);
            }
        }

        private void DeleteIntegrationPoints(IEnumerable<int> integrationPoints, int workspaceId)
        {
            MassDeleteByObjectIdentifiersRequest query = new MassDeleteByObjectIdentifiersRequest
            {
                Objects = new ReadOnlyCollection<RelativityObjectRef>(integrationPoints.Select(x => new RelativityObjectRef { ArtifactID = x }).ToList())
            };

            using (IObjectManager objectManager = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory
                .GetServiceProxy<IObjectManager>())
            {
                objectManager.DeleteAsync(workspaceId, query).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Returns measured time in seconds. Measures only time between job start and finish
        ///
        /// Job is started when status changes from Pending to anything else
        /// Job is ended when status is not Processing or Validating
        /// </summary>
        /// <returns>job time in seconds</returns>
        public async Task<double> RunSyncAndMeasureTime(int runIndex)
        {
            Workspace destinationWorkspace = _destinationWorkspaces[runIndex];

            IntegrationPointModel integrationPoint =
                await GetIntegrationPointAsync(destinationWorkspace).ConfigureAwait(false);
            await _ripApi.CreateIntegrationPointAsync(integrationPoint, SourceWorkspace.ArtifactID)
                .ConfigureAwait(false);

            _integrationPoints.Add(integrationPoint.ArtifactId);

            int jobHistoryId = await _ripApi.RunIntegrationPointAsync(integrationPoint, SourceWorkspace.ArtifactID)
                .ConfigureAwait(false);

            await WaitForJobToStartAsync(jobHistoryId, SourceWorkspace.ArtifactID, checkDelayInMs: 100).ConfigureAwait(false);
            Stopwatch stopwatch = Stopwatch.StartNew();

            await WaitForJobToFinishAsync(jobHistoryId, SourceWorkspace.ArtifactID, checkDelayInMs: 250);

            stopwatch.Stop();

            return stopwatch.Elapsed.Milliseconds / 1000d;
        }

        /// <summary>
        /// Runs <see cref="RunSyncAndMeasureTime"/> in a loop and calculates average time
        /// </summary>
        /// <param name="runCount"></param>
        /// <returns>Average execution time</returns>
        public async Task<double> RunPerformanceBenchmark(int runCount)
        {
            double runTimeSum = 0;
            for (int i = 0; i < runCount; i++)
            {
                runTimeSum += await RunSyncAndMeasureTime(i).ConfigureAwait(false);
            }

            return runTimeSum / runCount;
        }

        private Task WaitForJobToFinishAsync(int integrationPointId, int workspaceId, int checkDelayInMs = 500)
        {
            return WaitForJobStatus(integrationPointId, workspaceId, status =>
                status != PerformanceTestsConstants.JOB_STATUS_PROCESSING &&
                status != PerformanceTestsConstants.JOB_STATUS_VALIDATING, checkDelayInMs);
        }

        private Task WaitForJobToStartAsync(int jobHistoryId, int workspaceId, int checkDelayInMs = 500)
        {
            return WaitForJobStatus(jobHistoryId, workspaceId, status => status != PerformanceTestsConstants.JOB_STATUS_PENDING, checkDelayInMs);
        }

        private async Task WaitForJobStatus(int jobHistoryId, int workspaceId, Func<string, bool> waitUntil, int checkDelayInMs)
        {
            string status = await _ripApi.GetJobHistoryStatus(jobHistoryId, workspaceId);
            while (!waitUntil(status))
            {
                await Task.Delay(checkDelayInMs);
                status = await _ripApi.GetJobHistoryStatus(jobHistoryId, workspaceId).ConfigureAwait(false);
            }
        }

        private async Task<IntegrationPointModel> GetIntegrationPointAsync(Workspace destinationWorkspace)
        {
            _destinationWorkspaceDataService = new CommonIntegrationPointDataService(RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory, destinationWorkspace.ArtifactID);

            List<FieldMap> fieldsMapping = await _sourceWorkspaceDataService.GetIdentifierMappingAsync(destinationWorkspace.ArtifactID).ConfigureAwait(false);
            int rootFolderId = await _destinationWorkspaceDataService.GetRootFolderArtifactIdAsync().ConfigureAwait(false);            

            var sourceConfiguration = new RelativityProviderSourceConfiguration
            {
                TypeOfExport = (int)SourceConfiguration.ExportType.SavedSearch,
                SavedSearchArtifactId = _savedSearchId,
                SourceWorkspaceArtifactId = SourceWorkspace.ArtifactID,
                UseDynamicFolderPath = false
            };

            return new IntegrationPointModel
            {
                SourceConfiguration = sourceConfiguration,
                DestinationConfiguration =
                    GetDestinationConfiguration(destinationWorkspace.ArtifactID, rootFolderId),
                Name = string.Format(PerformanceTestsConstants.PERFORMANCE_TEST_INTEGRATION_POINT_NAME_FORMAT,
                    destinationWorkspace.ArtifactID),
                FieldMappings = fieldsMapping,
                DestinationProvider = _destinationProviderId,
                SourceProvider = _sourceProviderId,
                Type = _integrationPointType,
                EmailNotificationRecipients = "",
                OverwriteFieldsChoiceId = await _sourceWorkspaceDataService.GetOverwriteFieldsChoiceIdAsync("Append/Overlay").ConfigureAwait(false),
                ScheduleRule = new ScheduleModel()
            };
        }

        private async Task GetIntegrationPointsConstantsAsync()
        {
            _savedSearchId = await _sourceWorkspaceDataService.GetSavedSearchArtifactIdAsync(SavedSearchName).ConfigureAwait(false);          

            _destinationProviderId = await _sourceWorkspaceDataService.GetDestinationProviderIdAsync(DestinationProviders.RELATIVITY).ConfigureAwait(false);            

            _sourceProviderId = await _sourceWorkspaceDataService.GetSourceProviderIdAsync(SourceProviders.RELATIVITY).ConfigureAwait(false);           

            _integrationPointType = await _sourceWorkspaceDataService.GetIntegrationPointTypeByAsync(IntegrationPointTypes.ExportName).ConfigureAwait(false);             
        }        

        private RelativityProviderDestinationConfiguration GetDestinationConfiguration(int workspaceId, int folderId)
        {
            return new RelativityProviderDestinationConfiguration
            {
                CaseArtifactId = workspaceId,
                FieldOverlayBehavior = RelativityProviderValidationMessages.FIELD_MAP_FIELD_OVERLAY_BEHAVIOR_MERGE,
                ImportNativeFile = false,
                ArtifactTypeID = (int)ArtifactType.Document,
                DestinationFolderArtifactId = folderId,
                FolderPathSourceField = 0,
                UseFolderPathInformation = false
            };
        }        
    }
}
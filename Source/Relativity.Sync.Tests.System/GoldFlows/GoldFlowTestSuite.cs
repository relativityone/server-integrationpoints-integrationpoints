using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.ServiceProxy;
using Relativity.Services.Workspace;
using Relativity.Sync.Configuration;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.Common.RdoGuidProviderStubs;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Extensions;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Tests.System.Core.Runner;
using Relativity.Telemetry.APM;
using Relativity.Toggles;
using User = Relativity.Services.User.User;

namespace Relativity.Sync.Tests.System.GoldFlows
{
    internal class GoldFlowTestSuite
    {
        private static readonly Guid SourceCaseTagObjectTypeGuid = new Guid("7E03308C-0B58-48CB-AFA4-BB718C3F5CAC");
        private readonly TestEnvironment _environment;

        public User User { get; }

        public ServiceFactory ServiceFactory { get; }

        public WorkspaceRef SourceWorkspace { get; }

        private GoldFlowTestSuite(TestEnvironment environment, User user, ServiceFactory serviceFactory, WorkspaceRef sourceWorkspace)
        {
            _environment = environment;
            User = user;
            ServiceFactory = serviceFactory;
            SourceWorkspace = sourceWorkspace;
        }

        internal static async Task<GoldFlowTestSuite> CreateAsync(TestEnvironment environment, User user, ServiceFactory serviceFactory)
        {
            WorkspaceRef sourceWorkspace = await environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);
            return new GoldFlowTestSuite(environment, user, serviceFactory, sourceWorkspace);
        }

        public Task ImportDocumentsAsync(ImportDataTableWrapper importDataTable)
        {
            ImportHelper importHelper = new ImportHelper(ServiceFactory);
            return importHelper.ImportDataAsync(SourceWorkspace.ArtifactID, importDataTable);
        }

        public Task ImportDocumentsAsync(ImportDataTableWrapper importDataTable, WorkspaceRef workspace)
        {
            ImportHelper importHelper = new ImportHelper(ServiceFactory);
            return importHelper.ImportDataAsync(workspace.ArtifactID, importDataTable);
        }

        /// <summary>
        /// Creates gold flow tests run.
        /// </summary>
        /// <param name="configureAsync">Method used to set up configuration.</param>
        /// <param name="destinationWorkspaceID">Artifact ID of the existing destination workspace, or null to create new workspace.</param>
        public async Task<IGoldFlowTestRun> CreateTestRunAsync(Func<WorkspaceRef, WorkspaceRef, ConfigurationStub, Task> configureAsync, int? destinationWorkspaceID = null)
        {
            WorkspaceRef destinationWorkspace;

            if (destinationWorkspaceID.HasValue)
            {
                destinationWorkspace = await _environment.GetWorkspaceAsync(destinationWorkspaceID.Value).ConfigureAwait(false);
            }
            else
            {
                destinationWorkspace = await _environment.CreateWorkspaceAsync(templateWorkspaceName: SourceWorkspace.Name).ConfigureAwait(false);
            }

            ConfigurationStub configuration = new ConfigurationStub
            {
                CreateSavedSearchForTags = false
            };

            configuration.SetEmailNotificationRecipients(string.Empty);

            configuration.SourceWorkspaceArtifactId = SourceWorkspace.ArtifactID;
            configuration.DestinationWorkspaceArtifactId = destinationWorkspace.ArtifactID;
            configuration.SavedSearchArtifactId = await Rdos.GetSavedSearchInstanceAsync(ServiceFactory, SourceWorkspace.ArtifactID).ConfigureAwait(false);
            configuration.DataSourceArtifactId = configuration.SavedSearchArtifactId;
            configuration.JobHistoryArtifactId = await Rdos.CreateJobHistoryInstanceAsync(ServiceFactory, SourceWorkspace.ArtifactID, $"Sync Job {DateTime.Now:yyyy MMMM dd HH.mm.ss.fff}").ConfigureAwait(false);
            configuration.DestinationFolderArtifactId = await Rdos.GetRootFolderInstanceAsync(ServiceFactory, destinationWorkspace.ArtifactID).ConfigureAwait(false);

            await configureAsync(SourceWorkspace, destinationWorkspace, configuration).ConfigureAwait(false);

            configuration.SyncStatisticsId = await Rdos.CreateEmptySyncStatisticsRdoAsync(SourceWorkspace.ArtifactID).ConfigureAwait(false);

            int configurationId = await Rdos.CreateSyncConfigurationRdoAsync(SourceWorkspace.ArtifactID, configuration)
                .ConfigureAwait(false);

            return new GoldFlowTestRun(this, configurationId, configuration);
        }

        internal interface IGoldFlowTestRun
        {
            IToggleProvider ToggleProvider { get; }

            int DestinationWorkspaceArtifactId { get; }

            int SourceWorkspaceArtifactId { get; }

            Task<SyncJobState> RunAsync();

            Task AssertAsync(SyncJobState result, int expectedItemsTransferred, int expectedTotalItems, Guid? jobHistoryGuid = null);

            void AssertDocuments(string[] sourceDocumentsNames, string[] destinationDocumentsNames);

            void AssertImages(int sourceWorkspaceId, RelativityObject[] sourceWorkspaceDocuments, int destinationWorkspaceId, RelativityObject[] destinationWorkspaceDocumentIds);

            Task<bool> DocumentsHaveTagsAsync(string expectedTagValue);
        }

        private class GoldFlowTestRun : IGoldFlowTestRun
        {
            public IToggleProvider ToggleProvider { get; }

            private readonly GoldFlowTestSuite _goldFlowTestSuite;
            private readonly ConfigurationStub _configuration;
            private readonly SyncJobParameters _parameters;

            public int DestinationWorkspaceArtifactId => _configuration.DestinationWorkspaceArtifactId;

            public int SourceWorkspaceArtifactId => _configuration.SourceWorkspaceArtifactId;

            public GoldFlowTestRun(GoldFlowTestSuite goldFlowTestSuite, int configurationId, ConfigurationStub configuration)
            {
                ToggleProvider = new TestSyncToggleProvider();
                _goldFlowTestSuite = goldFlowTestSuite;
                _configuration = configuration;
                _parameters = new SyncJobParameters(configurationId, goldFlowTestSuite.SourceWorkspace.ArtifactID, configuration.ExecutingUserId, Guid.NewGuid(), Guid.Empty);
            }

            public Task<SyncJobState> RunAsync()
            {
                var syncRunner = new SyncRunner(AppSettings.RelativityUrl, new NullAPM(), TestLogHelper.GetLogger(), ToggleProvider);

                return syncRunner.RunAsync(_parameters, _goldFlowTestSuite.User.ArtifactID);
            }

            public async Task AssertAsync(SyncJobState result, int expectedItemsTransferred, int expectedTotalItems, Guid? jobHistoryGuid = null)
            {
                result.Status.Should().Be(SyncJobStatus.Completed, result.Message);
                jobHistoryGuid = jobHistoryGuid ?? DefaultGuids.JobHistory.TypeGuid;

                RelativityObject jobHistory = await Rdos
                    .GetJobHistoryAsync(_goldFlowTestSuite.ServiceFactory, _goldFlowTestSuite.SourceWorkspace.ArtifactID, _configuration.JobHistoryArtifactId, jobHistoryGuid.Value)
                    .ConfigureAwait(false);
                int itemsTransferred = (int)jobHistory["Items Transferred"].Value;
                int totalItems = (int)jobHistory["Total Items"].Value;

                using (var objectManager = _goldFlowTestSuite.ServiceFactory.CreateProxy<IObjectManager>())
                {
                    string aggregatedJobHistoryErrors =
                        await objectManager.AggregateJobHistoryErrorMessagesAsync(_goldFlowTestSuite.SourceWorkspace.ArtifactID, jobHistory.ArtifactID).ConfigureAwait(false);

                    aggregatedJobHistoryErrors.Should().BeNullOrEmpty("There should be no item level errors");
                }

                itemsTransferred.Should().Be(expectedItemsTransferred);
                totalItems.Should().Be(expectedTotalItems);
            }

            public void AssertDocuments(string[] sourceDocumentsNames, string[] destinationDocumentsNames)
            {
                var destinationDocumentsNamesSet = new HashSet<string>(destinationDocumentsNames);

                foreach (var name in sourceDocumentsNames)
                {
                    destinationDocumentsNamesSet.Contains(name).Should().BeTrue($"Document {name} was not created in destination workspace");
                }
            }

            public void AssertImages(int sourceWorkspaceId, RelativityObject[] sourceWorkspaceDocuments, int destinationWorkspaceId, RelativityObject[] destinationWorkspaceDocumentIds)
            {
                string GetExpectedIdentifier(string controlNumber, int index)
                {
                    if (index == 0)
                    {
                        return controlNumber;
                    }

                    return $"{controlNumber}_{index}";
                }

                ILookup<int, TestImageFile> sourceWorkspaceFiles;
                Dictionary<string, TestImageFile> destinationWorkspaceFiles;

                using (ISearchService searchManager = _goldFlowTestSuite.ServiceFactory.CreateProxy<ISearchService>())
                {
                    DataTable dataTable = searchManager
                        .RetrieveImagesForSearchAsync(sourceWorkspaceId, sourceWorkspaceDocuments.Select(x => x.ArtifactID).ToArray(), string.Empty)
                        .GetAwaiter().GetResult()
                        .Unwrap()
                        .Tables[0];

                    sourceWorkspaceFiles = dataTable
                        .AsEnumerable()
                        .Select(TestImageFile.GetFile)
                        .ToLookup(x => x.DocumentArtifactId, x => x);

                    destinationWorkspaceFiles = searchManager
                        .RetrieveImagesForSearchAsync(destinationWorkspaceId, destinationWorkspaceDocumentIds.Select(x => x.ArtifactID).ToArray(), string.Empty)
                        .GetAwaiter().GetResult()
                        .Unwrap()
                        .Tables[0]
                        .AsEnumerable()
                        .Select(TestImageFile.GetFile)
                        .ToDictionary(x => x.Identifier, x => x);
                }

                Dictionary<int, string> sourceWorkspaceDocumentNames = sourceWorkspaceDocuments.ToDictionary(x => x.ArtifactID, x => x.Name);

                foreach (IGrouping<int, TestImageFile> sourceDocumentImages in sourceWorkspaceFiles)
                {
                    int i = 0;
                    foreach (TestImageFile imageFile in sourceDocumentImages)
                    {
                        string expectedIdentifier = GetExpectedIdentifier(sourceWorkspaceDocumentNames[imageFile.DocumentArtifactId], i);

                        destinationWorkspaceFiles.ContainsKey(expectedIdentifier).Should()
                            .BeTrue($"Image [{sourceDocumentImages.Key} => {expectedIdentifier}] was not pushed to destination workspace");

                        TestImageFile destinationImage = destinationWorkspaceFiles[expectedIdentifier];

                        TestImageFile.AssertAreEquivalent(imageFile, destinationImage, expectedIdentifier);

                        if (_configuration.ImportImageFileCopyMode == ImportImageFileCopyMode.SetFileLinks)
                        {
                            TestImageFile.AssertImageIsLinked(imageFile, destinationImage);
                        }

                        i++;
                    }
                }
            }

            public async Task<bool> DocumentsHaveTagsAsync(string expectedTagValue)
            {
                QueryResult result;
                using (IObjectManager objectManager = _goldFlowTestSuite.ServiceFactory.CreateProxy<IObjectManager>())
                {
                    QueryRequest request = new QueryRequest()
                    {
                        ObjectType = new ObjectTypeRef { Guid = SourceCaseTagObjectTypeGuid },
                        IncludeNameInQueryResult = true
                    };
                    try
                    {
                        result = await objectManager.QueryAsync(DestinationWorkspaceArtifactId, request, 0, int.MaxValue).ConfigureAwait(false);
                    }
                    catch (NotFoundException)
                    {
                        return false;
                    }
                }

                return result.Objects.Any(x => x.Name == expectedTagValue);
            }
        }
    }
}

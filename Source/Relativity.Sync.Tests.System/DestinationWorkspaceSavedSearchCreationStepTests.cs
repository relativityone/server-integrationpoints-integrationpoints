using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Services.Search;
using Relativity.Services.Workspace;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Testing.Identification;

namespace Relativity.Sync.Tests.System
{
    [TestFixture]
    [Feature.DataTransfer.IntegrationPoints.Sync]
    internal class DestinationWorkspaceSavedSearchCreationStepTests : SystemTest
    {
        private WorkspaceRef _destinationWorkspace;
        private WorkspaceRef _sourceWorkspace;
        private const string _LOCAL_INSTANCE_NAME = "This Instance";

        [SetUp]
        public async Task SetUp()
        {
            _sourceWorkspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);
            _destinationWorkspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);
        }

        [IdentifiedTest("2db7474f-3032-49cb-92f3-3047c7604fd9")]
        public async Task ItShouldCreateSavedSearch()
        {
            // ARRANGE
            const int jobHistoryArtifactId = 456;
            string jobHistoryName = "Job History Tag Name";
            string sourceWorkspaceName = "Source Workspace";
            string sourceWorkspaceTagName = "Source Workspace Tag Name";
            string sourceJobTagName = "Source Job Tag Name";

            int sourceCaseTagArtifactId = await CreateRelativitySourceCaseTagAsync(sourceWorkspaceTagName, _sourceWorkspace.ArtifactID, sourceWorkspaceName).ConfigureAwait(false);
            int sourceJobTagArtifactId = await CreateSourceJobTagAsync(jobHistoryArtifactId, jobHistoryName, sourceCaseTagArtifactId, sourceJobTagName).ConfigureAwait(false);

            ConfigurationStub configuration = new ConfigurationStub
            {
                CreateSavedSearchForTags = true,
                DestinationWorkspaceArtifactId = _destinationWorkspace.ArtifactID,
                SourceWorkspaceArtifactId = _sourceWorkspace.ArtifactID,
                SourceJobTagArtifactId = sourceJobTagArtifactId
            };
            configuration.SetSourceJobTagName(sourceJobTagName);
            PrepareSyncConfigurationAndAssignId(configuration);

            ISyncJob syncJob = SyncJobHelper.CreateWithMockedProgressAndContainerExceptProvidedType<IDestinationWorkspaceSavedSearchCreationConfiguration>(configuration);
            
            // ACT
            await syncJob.ExecuteAsync(CompositeCancellationToken.None).ConfigureAwait(false);

            // ASSERT
            bool savedSearchExists = await DoesSavedSearchExistAsync(sourceJobTagName).ConfigureAwait(false);

            savedSearchExists.Should().BeTrue();
        }

        private async Task<int> CreateRelativitySourceCaseTagAsync(string sourceWorkspaceTagName, int sourceWorkspaceArtifactId, string sourceWorkspaceName)
        {
            RelativitySourceCaseTag sourceCaseTag = new RelativitySourceCaseTag
            {
                Name = sourceWorkspaceTagName,
                SourceWorkspaceArtifactId = sourceWorkspaceArtifactId,
                SourceWorkspaceName = sourceWorkspaceName,
                SourceInstanceName = _LOCAL_INSTANCE_NAME
            };

            int sourceCaseTagArtifactId = await Rdos.CreateRelativitySourceCaseInstanceAsync(ServiceFactory, _destinationWorkspace.ArtifactID, sourceCaseTag).ConfigureAwait(false);
            return sourceCaseTagArtifactId;
        }

        private async Task<int> CreateSourceJobTagAsync(int jobHistoryArtifactId, string jobHistoryName, int sourceCaseTagArtifactId, string sourceJobTagName)
        {
            RelativitySourceJobTag sourceJobTag = new RelativitySourceJobTag
            {
                JobHistoryArtifactId = jobHistoryArtifactId,
                JobHistoryName = jobHistoryName,
                SourceCaseTagArtifactId = sourceCaseTagArtifactId,
                Name = sourceJobTagName
            };

            int sourceJobTagArtifactId = await Rdos.CreateRelativitySourceJobInstanceAsync(ServiceFactory, _destinationWorkspace.ArtifactID, sourceJobTag).ConfigureAwait(false);
            return sourceJobTagArtifactId;
        }

        private async Task<bool> DoesSavedSearchExistAsync(string sourceJobTagName)
        {
            using (var savedSearchManager = ServiceFactory.CreateProxy<IKeywordSearchManager>())
            {
                Services.Query query = new Services.Query
                {
                    Condition = $"\"Name\" == \"{sourceJobTagName}\""
                };
                KeywordSearchQueryResultSet result = await savedSearchManager.QueryAsync(_destinationWorkspace.ArtifactID, query).ConfigureAwait(false);
                return result.Results[0].Artifact != null;
            }
        }
    }
}
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Services.Workspace;
using Relativity.Sync.Executors;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core;
using Relativity.Testing.Identification;

namespace Relativity.Sync.Tests.System
{
    [TestFixture]
    [Feature.DataTransfer.IntegrationPoints.Sync]
    internal sealed class RelativitySourceCaseTagRepositoryTests : SystemTest
    {
        private int _sourceWorkspaceArtifactId;
        private int _destinationWorkspaceArtifactId;
        private string _sourceWorkspaceName;
        private const string _INSTANCE_NAME = "c5212f20-bec4-426c-ad5c-8ebe2697cb19";

        protected override async Task ChildSuiteSetup()
        {
            WorkspaceRef sourceWorkspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);
            _sourceWorkspaceArtifactId = sourceWorkspace.ArtifactID;
            _sourceWorkspaceName = sourceWorkspace.Name;

            WorkspaceRef destinationWorkspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);
            _destinationWorkspaceArtifactId = destinationWorkspace.ArtifactID;
        }

        [IdentifiedTest("4D1CE900-A2A9-4388-852E-16C14AB6BADC")]
        public async Task RelativitySourceCaseTagReadShouldBeEqualToCreated()
        {
            //Arrange
            RelativitySourceCaseTagRepository repository =
                new RelativitySourceCaseTagRepository(new ServiceFactoryStub(ServiceFactory), SyncLog);

            RelativitySourceCaseTag relativitySourceCaseTag = new RelativitySourceCaseTag
            {
                ArtifactId = 432431,
                Name = "A7",
                SourceInstanceName = _INSTANCE_NAME,
                SourceWorkspaceArtifactId = _sourceWorkspaceArtifactId,
                SourceWorkspaceName = _sourceWorkspaceName
            };

            RelativitySourceCaseTag createdSourceCaseTag = await repository.CreateAsync(_destinationWorkspaceArtifactId, relativitySourceCaseTag);

            // Act
            RelativitySourceCaseTag sourceCaseTag = await repository.ReadAsync(_destinationWorkspaceArtifactId,
                _sourceWorkspaceArtifactId, _INSTANCE_NAME, CancellationToken.None)
                .ConfigureAwait(false);

            // Assert
            sourceCaseTag.Should().BeEquivalentTo(createdSourceCaseTag);
        }
    }
}

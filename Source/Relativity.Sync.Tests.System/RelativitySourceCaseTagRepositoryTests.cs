using System.Threading;
using System.Threading.Tasks;
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
        private const string _INSTANCE_NAME = "emttest";

        protected override async Task ChildSuiteSetup()
        {
            WorkspaceRef sourceWorkspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);
            _sourceWorkspaceArtifactId = sourceWorkspace.ArtifactID;

            WorkspaceRef destinationWorkspace = await Environment.CreateWorkspaceAsync().ConfigureAwait(false);
            _destinationWorkspaceArtifactId = destinationWorkspace.ArtifactID;
        }

        [IdentifiedTest("4D1CE900-A2A9-4388-852E-16C14AB6BADC")]
        public async Task SampleTest()
        {
            
            RelativitySourceCaseTagRepository repository =
                new RelativitySourceCaseTagRepository(new ServiceFactoryStub(ServiceFactory), Logger);

            RelativitySourceCaseTag sourceCaseTag = await repository.ReadAsync(_destinationWorkspaceArtifactId,
                _sourceWorkspaceArtifactId, _INSTANCE_NAME, new CancellationToken())
                .ConfigureAwait(false);

        }
    }
}

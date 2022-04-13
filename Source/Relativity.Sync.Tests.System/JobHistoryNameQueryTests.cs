using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Workspace;
using Relativity.Sync.Executors;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Testing.Identification;

namespace Relativity.Sync.Tests.System
{
    [TestFixture]
    [Feature.DataTransfer.IntegrationPoints.Sync]
    internal sealed class JobHistoryNameQueryTests : SystemTest
    {
        private int _sourceWorkspaceArtifactId;
        private RelativityObject _jobHistory;
        private Guid _jobHistoryObjectTypeGuid;
        
        protected override async Task ChildSuiteSetup()
        {
            _jobHistoryObjectTypeGuid = new Guid("08F4B1F7-9692-4A08-94AB-B5F3A88B6CC9");

            WorkspaceRef sourceWorkspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);
            _sourceWorkspaceArtifactId = sourceWorkspace.ArtifactID;

            _jobHistory = await Rdos.CreateJobHistoryRelativityObjectInstanceAsync(ServiceFactory, _sourceWorkspaceArtifactId, _jobHistoryObjectTypeGuid).ConfigureAwait(false);
        }

        [IdentifiedTest("A272AEC0-D482-437B-9B83-2650757A03F7")]
        public async Task RelativitySourceCaseTagReadShouldBeEqualToCreated()
        {
            //Arrange
            JobHistoryNameQuery jobHistoryNameQuery =
                new JobHistoryNameQuery(new ServiceFactoryStub(ServiceFactory), SyncLog);

            // Act
            string sourceCaseTag = await jobHistoryNameQuery.GetJobNameAsync(_jobHistoryObjectTypeGuid, 
                    _jobHistory.ArtifactID, _sourceWorkspaceArtifactId, CancellationToken.None)
                .ConfigureAwait(false);

            // Assert
            sourceCaseTag.Should().BeEquivalentTo(_jobHistory.FieldValues.Single().Value.ToString());
        }
    }
}


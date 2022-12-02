using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.Storage
{
    [TestFixture]
    internal sealed class SourceWorkspaceTagsCreationConfigurationTests : ConfigurationTestBase
    {
        private SourceWorkspaceTagsCreationConfiguration _config;

        private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 2;
        private const int _USER_ID = 323454;

        [SetUp]
        public void SetUp()
        {
            SyncJobParameters syncJobParameters = new SyncJobParameters(It.IsAny<int>(), _SOURCE_WORKSPACE_ARTIFACT_ID, _USER_ID, It.IsAny<Guid>(), Guid.Empty);
            _config = new SourceWorkspaceTagsCreationConfiguration(_configuration, syncJobParameters);
        }

        [Test]
        public void ItShouldReturnSourceWorkspaceArtifactId()
        {
            // act
            int srcWorkspaceArtifactId = _config.SourceWorkspaceArtifactId;

            // assert
            srcWorkspaceArtifactId.Should().Be(_SOURCE_WORKSPACE_ARTIFACT_ID);
        }

        [Test]
        public void ItShouldReturnDestinationWorkspaceArtifactId()
        {
            const int destinationWorkspaceArtifactId = 3;
            _configurationRdo.DestinationWorkspaceArtifactId = destinationWorkspaceArtifactId;

            // act
            int actualDestinationWorkspaceTagArtifactId = _config.DestinationWorkspaceArtifactId;

            // assert
            actualDestinationWorkspaceTagArtifactId.Should().Be(destinationWorkspaceArtifactId);
        }

        [Test]
        public void ItShouldReturnJobHistoryArtifactId()
        {
            const int jobHistoryArtifactId = 4;
            _configurationRdo.JobHistoryId = jobHistoryArtifactId;

            // act
            int actualJobHistoryArtifactId = _config.JobHistoryArtifactId;

            // assert
            actualJobHistoryArtifactId.Should().Be(jobHistoryArtifactId);
        }

        [Test]
        public async Task ItShouldSetSourceJobTag()
        {
            const int artifactId = 5;

            // act
            await _config.SetDestinationWorkspaceTagArtifactIdAsync(artifactId).ConfigureAwait(false);

            // assert
            _configurationRdo.DestinationWorkspaceTagArtifactId.Should().Be(artifactId);
            _config.IsDestinationWorkspaceTagArtifactIdSet.Should().BeTrue();
        }
    }
}

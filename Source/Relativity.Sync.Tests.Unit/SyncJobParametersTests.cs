using System;
using FluentAssertions;
using NUnit.Framework;

namespace Relativity.Sync.Tests.Unit
{
    [TestFixture]
    public class SyncJobParametersTests
    {
        [Test]
        public void WorkflowId_ShouldNotBeEmpty()
        {
            // Arrange
            SyncJobParameters syncJobParameters = new SyncJobParameters(1, 1, 1, Guid.NewGuid(), Guid.Empty);

            // Assert
            syncJobParameters.WorkflowId.Should().NotBeNullOrWhiteSpace();
        }

        [Test]
        public void WorkflowId_ShouldBeInitializedWithGivenValue()
        {
            // Arrange
            Guid workflowId = Guid.NewGuid();

            SyncJobParameters syncJobParameters = new SyncJobParameters(1, 1, 1, workflowId, Guid.Empty);

            // Assert
            syncJobParameters.WorkflowId.Should().Be(workflowId.ToString());
        }

        [Test]
        public void SyncConfigurationArtifactId_ShouldBeJobId()
        {
            // Arrange
            const int jobId = 801314;

            SyncJobParameters syncJobParameters = new SyncJobParameters(jobId, 1, 1, Guid.NewGuid(), Guid.Empty);

            // Assert
            syncJobParameters.SyncConfigurationArtifactId.Should().Be(jobId);
        }

        [Test]
        public void WorkspaceId_ShouldReturnWorkspaceId()
        {
            // Arrange
            const int workspaceId = 172320;

            SyncJobParameters syncJobParameters = new SyncJobParameters(1, workspaceId, 1, Guid.NewGuid(), Guid.Empty);

            // Assert
            syncJobParameters.WorkspaceId.Should().Be(workspaceId);
        }

        [Test]
        public void UserId_ShouldReturnUserId()
        {
            // Arrange
            const int userId = 4563456;

            SyncJobParameters syncJobParameters = new SyncJobParameters(1, 1, userId, Guid.NewGuid(), Guid.Empty);

            // Assert
            syncJobParameters.UserId.Should().Be(userId);
        }

        [Test]
        public void WorkflowId_ShouldBeNonEmptyWorkflowId()
        {
            // Arrange
            SyncJobParameters syncJobParameters = new SyncJobParameters(1, 1, 1, Guid.NewGuid(), Guid.Empty);

            // Assert
            syncJobParameters.WorkflowId.Should().NotBeNullOrWhiteSpace();
        }

        [Test]
        public void SyncJobParameters_ShouldCreateDifferentWorkflowIdsForDifferentJobs()
        {
            // Arrange
#pragma warning disable RG2009 // Hardcoded Numeric Value
            SyncJobParameters firstSyncJobParameters = new SyncJobParameters(1, 1, 1, Guid.NewGuid(), Guid.Empty);
            SyncJobParameters secondSyncJobParameters = new SyncJobParameters(1, 1, 1, Guid.NewGuid(), Guid.Empty);

            // Assert
            firstSyncJobParameters.WorkflowId.Should().NotBe(secondSyncJobParameters.WorkflowId);
#pragma warning restore RG2009 // Hardcoded Numeric Value
        }

        [Test]
        public void SyncApplicationName_ShouldHaveDefaultValue()
        {
            // Arrange
            SyncJobParameters syncJobParameters = new SyncJobParameters(0, 0, 0, Guid.Empty, Guid.Empty);

            // Assert
            syncJobParameters.SyncApplicationName.Should().Be("Relativity.Sync");
        }
    }
}

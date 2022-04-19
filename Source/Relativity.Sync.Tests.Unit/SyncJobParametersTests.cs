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
			SyncJobParameters syncJobParameters = new SyncJobParameters(1, 1, Guid.NewGuid());

			// Assert
			syncJobParameters.WorkflowId.Should().NotBeNullOrWhiteSpace();
		}

		[Test]
		public void WorkflowId_ShouldBeInitializedWithGivenValue()
		{
			// Arrange
			Guid workflowId = Guid.NewGuid();

			SyncJobParameters syncJobParameters = new SyncJobParameters(1, 1, workflowId);

			// Assert
			syncJobParameters.WorkflowId.Should().Be(workflowId.ToString());
		}

		[Test]
		public void SyncConfigurationArtifactId_ShouldBeJobId()
		{
			// Arrange
			const int jobId = 801314;

			SyncJobParameters syncJobParameters = new SyncJobParameters(jobId, 1, Guid.NewGuid());

			// Assert
			syncJobParameters.SyncConfigurationArtifactId.Should().Be(jobId);
		}

		[Test]
		public void WorkspaceId_ShouldReturnWorkspaceId()
		{
			// Arrange
			const int workspaceId = 172320;

			SyncJobParameters syncJobParameters = new SyncJobParameters(1, workspaceId, Guid.NewGuid());

			// Assert
			syncJobParameters.WorkspaceId.Should().Be(workspaceId);
		}

		[Test]
		public void WorkflowId_ShouldBeNonEmptyWorkflowId()
		{
			// Arrange
			SyncJobParameters syncJobParameters = new SyncJobParameters(1, 1, Guid.NewGuid());

			// Assert
			syncJobParameters.WorkflowId.Should().NotBeNullOrWhiteSpace();
		}

		[Test]
		public void SyncJobParameters_ShouldCreateDifferentWorkflowIdsForDifferentJobs()
		{
			// Arrange
#pragma warning disable RG2009 // Hardcoded Numeric Value
			SyncJobParameters firstSyncJobParameters = new SyncJobParameters(1, 1, Guid.NewGuid());
			SyncJobParameters secondSyncJobParameters = new SyncJobParameters(1, 1, Guid.NewGuid());

			// Assert
			firstSyncJobParameters.WorkflowId.Should().NotBe(secondSyncJobParameters.WorkflowId);
#pragma warning restore RG2009 // Hardcoded Numeric Value
		}

		[Test]
		public void SyncApplicationName_ShouldHaveDefaultValue()
		{
			// Arrange
			SyncJobParameters syncJobParameters = new SyncJobParameters(0, 0, Guid.Empty);

			// Assert
			syncJobParameters.SyncApplicationName.Should().Be("Relativity.Sync");
		}
	}
}
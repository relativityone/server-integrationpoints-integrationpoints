using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public class SyncJobParametersTests
	{
		[Test]
		public void WorkflowId_ShouldNotBeEmpty()
		{
			// Arrange
			SyncJobParameters syncJobParameters = new SyncJobParameters(1, 1, 1);

			// Assert
			syncJobParameters.WorkflowId.Value.Should().NotBeNullOrWhiteSpace();
		}

		[Test]
		public void WorkflowId_ShouldBeInitializedWithGivenValue()
		{
			// Arrange
			string workflowId = $"{TelemetryConstants.PROVIDER_NAME}_1";

			SyncJobParameters syncJobParameters = new SyncJobParameters(1, 1, 1);

			// Assert
			syncJobParameters.WorkflowId.Value.Should().Be(workflowId);
		}

		[Test]
		public void SyncConfigurationArtifactId_ShouldBeJobId()
		{
			// Arrange
			const int jobId = 801314;

			SyncJobParameters syncJobParameters = new SyncJobParameters(jobId, 1, 1);

			// Assert
			syncJobParameters.SyncConfigurationArtifactId.Should().Be(jobId);
		}

		[Test]
		public void WorkspaceId_ShouldReturnWorkspaceId()
		{
			// Arrange
			const int workspaceId = 172320;

			SyncJobParameters syncJobParameters = new SyncJobParameters(1, workspaceId, 1);

			// Assert
			syncJobParameters.WorkspaceId.Should().Be(workspaceId);
		}

		[Test]
		public void WorkflowId_ShouldBeNonEmptyWorkflowId()
		{
			// Arrange
			SyncJobParameters syncJobParameters = new SyncJobParameters(1, 1, 1);

			// Assert
			syncJobParameters.WorkflowId.Value.Should().NotBeNullOrWhiteSpace();
		}

		[Test]
		public void SyncJobParameters_ShouldCreateDifferentWorkflowIdsForDifferentJobs()
		{
			// Arrange
#pragma warning disable RG2009 // Hardcoded Numeric Value
			SyncJobParameters firstSyncJobParameters = new SyncJobParameters(1, 1, 1);
			SyncJobParameters secondSyncJobParameters = new SyncJobParameters(1, 1, 2);

			// Assert
			firstSyncJobParameters.WorkflowId.Value.Should().NotBe(secondSyncJobParameters.WorkflowId.Value);
#pragma warning restore RG2009 // Hardcoded Numeric Value
		}

		[Test]
		public void SyncApplicationName_ShouldHaveDefaultValue()
		{
			// Arrange
			SyncJobParameters syncJobParameters = new SyncJobParameters(0, 0, 0);

			// Assert
			syncJobParameters.SyncApplicationName.Should().Be("Relativity.Sync");
		}
	}
}
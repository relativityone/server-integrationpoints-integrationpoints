using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public class SyncJobParametersTests
	{
		[Test]
		public void WorkflowIdShouldNotBeEmpty()
		{
			SyncJobParameters syncJobParameters = new SyncJobParameters(1, 1, 1, 1);

			// ASSERT
			syncJobParameters.WorkflowId.Value.Should().NotBeNullOrWhiteSpace();
		}

		[Test]
		public void WorkflowIdShouldBeInitializedWithGivenValue()
		{
			string workflowId = $"{TelemetryConstants.PROVIDER_NAME}_1_1";

			SyncJobParameters syncJobParameters = new SyncJobParameters(1, 1, 1, 1);

			// ASSERT
			syncJobParameters.WorkflowId.Value.Should().Be(workflowId);
		}

		[Test]
		public void ItShouldSetJobId()
		{
			const int jobId = 801314;

			SyncJobParameters syncJobParameters = new SyncJobParameters(jobId, 1, 1, 1);

			// ASSERT
			syncJobParameters.SyncConfigurationArtifactId.Should().Be(jobId);
		}

		[Test]
		public void ItShouldSetWorkspaceId()
		{
			const int workspaceId = 172320;

			SyncJobParameters syncJobParameters = new SyncJobParameters(1, workspaceId, 1, 1);

			// ASSERT
			syncJobParameters.WorkspaceId.Should().Be(workspaceId);
		}

		[Test]
		public void ItShouldCreateNonEmptyWorkflowId()
		{
			SyncJobParameters syncJobParameters = new SyncJobParameters(1, 1, 1, 1);

			syncJobParameters.WorkflowId.Value.Should().NotBeNullOrWhiteSpace();
		}
		
		[Test]
		public void ItShouldCreateDifferentWorkflowIdsForDifferentJobs()
		{
#pragma warning disable RG2009 // Hardcoded Numeric Value
			SyncJobParameters firstSyncJobParameters = new SyncJobParameters(1, 1, 1, 1);
			SyncJobParameters secondSyncJobParameters = new SyncJobParameters(1, 1, 1, 2);

			firstSyncJobParameters.WorkflowId.Value.Should().NotBe(secondSyncJobParameters.WorkflowId.Value);
#pragma warning restore RG2009 // Hardcoded Numeric Value
		}
	}
}
using System;
using FluentAssertions;
using NUnit.Framework;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public class SyncJobParametersTests
	{
		[Test]
		public void CorrelationIdShouldNotBeEmpty()
		{
			SyncJobParameters syncJobParameters = new SyncJobParameters(1, 1);

			// ASSERT
			syncJobParameters.CorrelationId.Should().NotBeNullOrWhiteSpace();
		}

		[Test]
		public void CorrelationIdShouldBeInitializedWithGivenValue()
		{
			const string id = "example id";

			SyncJobParameters syncJobParameters = new SyncJobParameters(1, 1, 1, id);

			// ASSERT
			syncJobParameters.CorrelationId.Should().Be(id);
		}

		[Test]
		public void ItShouldSetJobId()
		{
			const int jobId = 801314;

			SyncJobParameters syncJobParameters = new SyncJobParameters(jobId, 1);

			// ASSERT
			syncJobParameters.SyncConfigurationArtifactId.Should().Be(jobId);
		}

		[Test]
		public void ItShouldSetWorkspaceId()
		{
			const int workspaceId = 172320;

			SyncJobParameters syncJobParameters = new SyncJobParameters(1, workspaceId);

			// ASSERT
			syncJobParameters.WorkspaceId.Should().Be(workspaceId);
		}

		[Test]
		public void ItShouldCreateNonEmptyCorrelationId()
		{
			SyncJobParameters syncJobParameters = new SyncJobParameters(1, 1);

			syncJobParameters.CorrelationId.Should().NotBeNullOrWhiteSpace();
			syncJobParameters.CorrelationId.Should().NotBe(new Guid().ToString());
		}



		[Test]
		public void ItShouldCreateDifferentCorrelationIdsEveryTime()
		{
			SyncJobParameters firstSyncJobParameters = new SyncJobParameters(1, 1);
			SyncJobParameters secondSyncJobParameters = new SyncJobParameters(1, 1);

			firstSyncJobParameters.CorrelationId.Should().NotBe(secondSyncJobParameters.CorrelationId);
		}
	}
}
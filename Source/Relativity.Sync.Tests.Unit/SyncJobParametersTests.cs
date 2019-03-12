using System;
using FluentAssertions;
using NUnit.Framework;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public static class SyncJobParametersTests
	{
		[Test]
		public static void CorrelationIdShouldNotBeEmpty()
		{
			SyncJobParameters syncJobParameters = new SyncJobParameters(1, 1);

			// ASSERT
			syncJobParameters.CorrelationId.Should().NotBeNullOrWhiteSpace();
		}

		[Test]
		public static void CorrelationIdShouldBeInitializedWithGivenValue()
		{
			const string id = "example id";

			SyncJobParameters syncJobParameters = new SyncJobParameters(1, 1, id);

			// ASSERT
			syncJobParameters.CorrelationId.Should().Be(id);
		}

		[Test]
		public static void ItShouldSetJobId()
		{
			const int jobId = 801314;

			SyncJobParameters syncJobParameters = new SyncJobParameters(jobId, 1);

			// ASSERT
			syncJobParameters.JobId.Should().Be(jobId);
		}

		[Test]
		public static void ItShouldSetWorkspaceId()
		{
			const int workspaceId = 172320;

			SyncJobParameters syncJobParameters = new SyncJobParameters(1, workspaceId);

			// ASSERT
			syncJobParameters.WorkspaceId.Should().Be(workspaceId);
		}

		[Test]
		public static void ItShouldCreateNonEmptyCorrelationId()
		{
			SyncJobParameters syncJobParameters = new SyncJobParameters(1, 1);

			syncJobParameters.CorrelationId.Should().NotBeNullOrWhiteSpace();
			syncJobParameters.CorrelationId.Should().NotBe(new Guid().ToString());
		}



		[Test]
		public static void ItShouldCreateDifferentCorrelationIdsEveryTime()
		{
			SyncJobParameters firstSyncJobParameters = new SyncJobParameters(1, 1);
			SyncJobParameters secondSyncJobParameters = new SyncJobParameters(1, 1);

			firstSyncJobParameters.CorrelationId.Should().NotBe(secondSyncJobParameters.CorrelationId);
		}
	}
}
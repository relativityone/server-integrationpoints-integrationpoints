using System;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public class SyncJobParametersTests
	{
		private ImportSettingsDto _importSettingsDto;

		[SetUp]
		public void SetUp()
		{
			_importSettingsDto = new ImportSettingsDto();
		}

		[Test]
		public void CorrelationIdShouldNotBeEmpty()
		{
			SyncJobParameters syncJobParameters = new SyncJobParameters(1, 1, _importSettingsDto);

			// ASSERT
			syncJobParameters.CorrelationId.Should().NotBeNullOrWhiteSpace();
		}

		[Test]
		public void CorrelationIdShouldBeInitializedWithGivenValue()
		{
			const string id = "example id";

			SyncJobParameters syncJobParameters = new SyncJobParameters(1, 1, 1, id, _importSettingsDto);

			// ASSERT
			syncJobParameters.CorrelationId.Should().Be(id);
		}

		[Test]
		public void ItShouldSetJobId()
		{
			const int jobId = 801314;

			SyncJobParameters syncJobParameters = new SyncJobParameters(jobId, 1, _importSettingsDto);

			// ASSERT
			syncJobParameters.JobId.Should().Be(jobId);
		}

		[Test]
		public void ItShouldSetWorkspaceId()
		{
			const int workspaceId = 172320;

			SyncJobParameters syncJobParameters = new SyncJobParameters(1, workspaceId, _importSettingsDto);

			// ASSERT
			syncJobParameters.WorkspaceId.Should().Be(workspaceId);
		}

		[Test]
		public void ItShouldCreateNonEmptyCorrelationId()
		{
			SyncJobParameters syncJobParameters = new SyncJobParameters(1, 1, _importSettingsDto);

			syncJobParameters.CorrelationId.Should().NotBeNullOrWhiteSpace();
			syncJobParameters.CorrelationId.Should().NotBe(new Guid().ToString());
		}



		[Test]
		public void ItShouldCreateDifferentCorrelationIdsEveryTime()
		{
			SyncJobParameters firstSyncJobParameters = new SyncJobParameters(1, 1, _importSettingsDto);
			SyncJobParameters secondSyncJobParameters = new SyncJobParameters(1, 1, _importSettingsDto);

			firstSyncJobParameters.CorrelationId.Should().NotBe(secondSyncJobParameters.CorrelationId);
		}
	}
}
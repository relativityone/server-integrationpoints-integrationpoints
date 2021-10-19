using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.Storage
{
	
	internal class JobStatusConsolidationConfigurationTests : ConfigurationTestBase
	{
		private const int _WORKSPACE_ARTIFACT_ID = 567765;

		[Test]
		public void GetSourceWorkspaceArtifactId_ShouldRetrieveSourceWorkspaceArtifactId()
		{
			// Arrange
			const int expected = 567765;

			JobStatusConsolidationConfiguration sut = PrepareSut( syncConfigurationArtifactId: 0);

			// Act
			int sourceWorkspaceArtifactId = sut.SourceWorkspaceArtifactId;

			// Assert
			sourceWorkspaceArtifactId.Should().Be(expected);
		}

		[Test]
		public void GetSyncConfigurationArtifactId_ShouldRetrieveSyncConfigurationArtifactId()
		{
			// Arrange
			const int syncConfigurationArtifactID = 123456;

			JobStatusConsolidationConfiguration sut = PrepareSut( syncConfigurationArtifactId: syncConfigurationArtifactID);

			// Act
			int syncConfigurationArtifactId = sut.SyncConfigurationArtifactId;

			// Assert
			syncConfigurationArtifactId.Should().Be(syncConfigurationArtifactID);
		}

		[Test]
		public void GetJobHistoryArtifactId_ShouldRetrieveJobHistoryArtifactId()
		{
			// Arrange
			const int jobHistoryArtifactID = 123456;

			_configurationRdo.JobHistoryId = jobHistoryArtifactID;
			
			JobStatusConsolidationConfiguration sut = PrepareSut(0);

			// Act
			int jobHistoryArtifactId = sut.JobHistoryArtifactId;

			// Assert
			jobHistoryArtifactId.Should().Be(jobHistoryArtifactID);
		}

		private JobStatusConsolidationConfiguration PrepareSut(int syncConfigurationArtifactId)
		{
			var syncJobParameters = new SyncJobParameters(syncConfigurationArtifactId, _WORKSPACE_ARTIFACT_ID, It.IsAny<Guid>());
			var sut = new JobStatusConsolidationConfiguration(_configuration, syncJobParameters);
			return sut;
		}
	}
}
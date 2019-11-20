using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.Storage
{
	[TestFixture]
	internal class JobStatusConsolidationConfigurationTests
	{
		private const int _WORKSPACE_ARTIFACT_ID = 567765;
		private static readonly Guid JobHistoryGuid = new Guid("5D8F7F01-25CF-4246-B2E2-C05882539BB2");

		[Test]
		public void GetSourceWorkspaceArtifactId_ShouldRetrieveSourceWorkspaceArtifactId()
		{
			// Arrange
			const int expected = 567765;

			JobStatusConsolidationConfiguration sut = PrepareSut(c => { }, syncConfigurationArtifactId: 0);

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

			JobStatusConsolidationConfiguration sut = PrepareSut(c => { }, syncConfigurationArtifactId: syncConfigurationArtifactID);

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

			JobStatusConsolidationConfiguration sut = PrepareSut(c => c
				.Setup(x => x.GetFieldValue<RelativityObjectValue>(It.Is<Guid>(g => g.Equals(JobHistoryGuid))))
				.Returns(new RelativityObjectValue { ArtifactID = jobHistoryArtifactID }),
				syncConfigurationArtifactId: 0);

			// Act
			int jobHistoryArtifactId = sut.JobHistoryArtifactId;

			// Assert
			jobHistoryArtifactId.Should().Be(jobHistoryArtifactID);
		}

		private JobStatusConsolidationConfiguration PrepareSut(Action<Mock<Sync.Storage.IConfiguration>> setupConfigurationMockAction, int syncConfigurationArtifactId)
		{
			var configuration = new Mock<Sync.Storage.IConfiguration>();
			setupConfigurationMockAction(configuration);

			var syncJobParameters = new SyncJobParameters(syncConfigurationArtifactId, _WORKSPACE_ARTIFACT_ID, 1, 1);
			var sut = new JobStatusConsolidationConfiguration(configuration.Object, syncJobParameters);
			return sut;
		}
	}
}
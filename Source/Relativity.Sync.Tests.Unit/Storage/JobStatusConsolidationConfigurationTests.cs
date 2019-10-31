using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.Storage
{
	[TestFixture]
	internal class JobStatusConsolidationConfigurationTests
	{
		private static readonly Guid _JOB_HISTORY_GUID = new Guid("5D8F7F01-25CF-4246-B2E2-C05882539BB2");

		[Test]
		public void GetSourceWorkspaceArtifactId_ShouldRetrieveSourceWorkspaceArtifactId()
		{
			// Arrange
			const int expected = 567765;

			IJobStatusConsolidationConfiguration sut = PrepareSut(c => { },
				syncConfigurationArtifactId: 0,
				sourceWorkspaceId: expected);

			// Act
			int sourceWorkspaceArtifactId = sut.SourceWorkspaceArtifactId;

			// Assert
			sourceWorkspaceArtifactId.Should().Be(expected);
		}

		[Test]
		public void GetSyncConfigurationArtifactId_ShouldRetrieveSyncConfigurationArtifactId()
		{
			// Arrange
			const int expected = 123456;
			const int sourceWorkspaceId = 567765;

			IJobStatusConsolidationConfiguration sut = PrepareSut(c => { },
				syncConfigurationArtifactId: expected,
				sourceWorkspaceId: sourceWorkspaceId);

			// Act
			int syncConfigurationArtifactId = sut.SyncConfigurationArtifactId;

			// Assert
			syncConfigurationArtifactId.Should().Be(expected);
		}

		[Test]
		public void GetJobHistoryArtifactId_ShouldRetrieveJobHistoryArtifactId()
		{
			// Arrange
			const int expected = 123456;
			const int sourceWorkspaceId = 567765;

			IJobStatusConsolidationConfiguration sut = PrepareSut(c => c
				.Setup(x => x.GetFieldValue<RelativityObjectValue>(It.Is<Guid>(g => g.Equals(_JOB_HISTORY_GUID))))
				.Returns(new RelativityObjectValue { ArtifactID = expected }),
				syncConfigurationArtifactId: 0,
				sourceWorkspaceId: sourceWorkspaceId);

			// Act
			int jobHistoryArtifactId = sut.JobHistoryArtifactId;

			// Assert
			jobHistoryArtifactId.Should().Be(expected);
		}

		public IJobStatusConsolidationConfiguration PrepareSut(Action<Mock<Sync.Storage.IConfiguration>> configurationMockConfiguration, int syncConfigurationArtifactId, int sourceWorkspaceId)
		{
			Mock<Sync.Storage.IConfiguration> configuration = new Mock<Sync.Storage.IConfiguration>();
			configurationMockConfiguration(configuration);

			var syncJobParameters = new SyncJobParameters(syncConfigurationArtifactId, sourceWorkspaceId);
			IJobStatusConsolidationConfiguration sut = new JobStatusConsolidationConfiguration(configuration.Object, syncJobParameters);
			return sut;
		}
	}
}
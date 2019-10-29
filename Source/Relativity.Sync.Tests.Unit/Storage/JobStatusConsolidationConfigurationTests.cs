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
		private static readonly Guid _SOURCE_WORKSPACE_TAG_ARTIFACT_ID_GUID = new Guid("FEAB129B-AEEF-4AA4-BC91-9EAE9A4C35F6");

		[Test]
		public void ItShouldRetrieveSourceWorkspaceArtifactId()
		{
			// Arrange
			const int expected = 123456;

			IJobStatusConsolidationConfiguration sut = PrepareSut(c => c
				.Setup(x => x.GetFieldValue<int>(It.Is<Guid>(g => g.Equals(_SOURCE_WORKSPACE_TAG_ARTIFACT_ID_GUID))))
				.Returns(expected), 0);

			// Act
			int sourceWorkspaceArtifactId = sut.SourceWorkspaceArtifactId;

			// Assert
			sourceWorkspaceArtifactId.Should().Be(expected);
		}

		[Test]
		public void ItShouldRetrieveSyncConfigurationArtifactId()
		{
			// Arrange
			const int expected = 123456;
			IJobStatusConsolidationConfiguration sut = PrepareSut(c => { }, expected);

			// Act
			int syncConfigurationArtifactId = sut.SyncConfigurationArtifactId;

			// Assert
			syncConfigurationArtifactId.Should().Be(expected);
		}

		[Test]
		public void ItShouldRetrieveJobHistoryArtifactId()
		{
			// Arrange
			const int expected = 123456;
			IJobStatusConsolidationConfiguration sut = PrepareSut(c => c
				.Setup(x => x.GetFieldValue<RelativityObjectValue>(It.Is<Guid>(g => g.Equals(_JOB_HISTORY_GUID))))
				.Returns(new RelativityObjectValue { ArtifactID = expected }), 0);

			// Act
			int jobHistoryArtifactId = sut.JobHistoryArtifactId;

			// Assert
			jobHistoryArtifactId.Should().Be(expected);
		}

		public IJobStatusConsolidationConfiguration PrepareSut(Action<Mock<Sync.Storage.IConfiguration>> configurationMockConfiguration, int syncConfigurationArtifactId)
		{
			Mock<Sync.Storage.IConfiguration> configuration = new Mock<Sync.Storage.IConfiguration>();
			configurationMockConfiguration(configuration);

			const int workspaceId = 999999;
			var syncJobParameters = new SyncJobParameters(syncConfigurationArtifactId, workspaceId);
			IJobStatusConsolidationConfiguration sut = new JobStatusConsolidationConfiguration(configuration.Object, syncJobParameters);
			return sut;
		}
	}
}
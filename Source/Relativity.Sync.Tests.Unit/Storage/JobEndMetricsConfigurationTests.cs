using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Storage;
using System;

namespace Relativity.Sync.Tests.Unit.Storage
{
	[TestFixture]
	public class JobEndMetricsConfigurationTests
	{
		private Mock<IConfiguration> _configurationFake;
		private JobEndMetricsConfiguration _sut;

		private readonly Guid _jobHistoryToRetryGuid = new Guid("D7D0DDB9-D383-4578-8D7B-6CBDD9E71549");

		private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 102779;
		private const int _SYNC_CONFIGURATION_ARTIFACT_ID = 103799;
		private const int _JOB_HISTORY_TO_RETRY_ARTIFACT_ID = 104799;

		[SetUp]
		public void SetUp()
		{
			_configurationFake = new Mock<IConfiguration>();
			_configurationFake.Setup(x => x.GetFieldValue<RelativityObjectValue>(_jobHistoryToRetryGuid))
				.Returns(new RelativityObjectValue() { ArtifactID = _JOB_HISTORY_TO_RETRY_ARTIFACT_ID });

			SyncJobParameters syncJobParameters = new SyncJobParameters(_SYNC_CONFIGURATION_ARTIFACT_ID, _SOURCE_WORKSPACE_ARTIFACT_ID, 1);
			_sut = new JobEndMetricsConfiguration(_configurationFake.Object, syncJobParameters);
		}

		[Test]
		public void SourceWorkspaceArtifactId_ShouldReturnProperValue()
		{
			// Act
			int sourceWorkspaceArtifactId = _sut.SourceWorkspaceArtifactId;

			// Assert
			sourceWorkspaceArtifactId.Should().Be(_SOURCE_WORKSPACE_ARTIFACT_ID);
		}

		[Test]
		public void SyncConfigurationArtifactId_ShouldReturnProperValue()
		{
			// Act
			int syncConfigurationArtifactId = _sut.SyncConfigurationArtifactId;

			// Assert
			syncConfigurationArtifactId.Should().Be(_SYNC_CONFIGURATION_ARTIFACT_ID);
		}

		[Test]
		public void JobHistoryToRetryId_ShouldReturnProperValue()
		{
			// Act
			int? jobHistoryToRetryId = _sut.JobHistoryToRetryId;

			// Assert
			jobHistoryToRetryId.Should().Be(_JOB_HISTORY_TO_RETRY_ARTIFACT_ID);
		}
	}
}
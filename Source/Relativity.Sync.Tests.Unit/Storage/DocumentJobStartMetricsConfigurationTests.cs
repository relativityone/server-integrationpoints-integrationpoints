using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.Storage
{
	internal class DocumentJobStartMetricsConfigurationTests : ConfigurationTestBase
	{
		private DocumentJobStartMetricsConfiguration _sut;

		private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 102779;

		[SetUp]
		public void SetUp()
		{

			_sut = new DocumentJobStartMetricsConfiguration(_configuration.Object, new SyncJobParameters(1, _SOURCE_WORKSPACE_ARTIFACT_ID, 1));
		}

		[Test]
		public void SourceWorkspaceArtifactId_ShouldReturnProperValue()
		{
			// Act & Assert
			_sut.SourceWorkspaceArtifactId.Should().Be(_SOURCE_WORKSPACE_ARTIFACT_ID);
		}

		[Test]
		public void JobHistoryToRetryId_ShouldReturnProperValue()
		{
			// Arrange
			const int jobHistoryArtifactId = 104799;
			_configurationRdo.JobHistoryToRetryId = jobHistoryArtifactId;

			// Act
			_sut.JobHistoryToRetryId.Should().Be(jobHistoryArtifactId);
		}

		[Test]
		public void DestinationWorkspaceArtifactId_ShouldReturnProperValue()
		{
			// Arrange
			const int destinationWorkspaceArtifactId = 106799;
			_configurationRdo.DestinationWorkspaceArtifactId = destinationWorkspaceArtifactId;

			// Act & Assert
			_sut.DestinationWorkspaceArtifactId.Should().Be(destinationWorkspaceArtifactId);
		}
	}
}

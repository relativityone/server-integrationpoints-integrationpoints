using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.Storage
{
	[TestFixture]
	public class JobEndMetricsConfigurationTests
	{
		private JobEndMetricsConfiguration _instance;
		
		private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 102779;
		private const int _SYNC_CONFIGURATION_ARTIFACT_ID = 103799;
		
		[SetUp]
		public void SetUp()
		{
			SyncJobParameters syncJobParameters = new SyncJobParameters(_SYNC_CONFIGURATION_ARTIFACT_ID, _SOURCE_WORKSPACE_ARTIFACT_ID, 1, 1);
			_instance = new JobEndMetricsConfiguration(syncJobParameters);
		}

		[Test]
		public void SourceWorkspaceArtifactId_ShouldReturnProperValue()
		{
			// Act
			int sourceWorkspaceArtifactId = _instance.SourceWorkspaceArtifactId;

			// Assert
			sourceWorkspaceArtifactId.Should().Be(_SOURCE_WORKSPACE_ARTIFACT_ID);
		}

		[Test]
		public void SyncConfigurationArtifactId_ShouldReturnProperValue()
		{
			// Act
			int syncConfigurationArtifactId = _instance.SyncConfigurationArtifactId;

			// Assert
			syncConfigurationArtifactId.Should().Be(_SYNC_CONFIGURATION_ARTIFACT_ID);
		}
	}
}
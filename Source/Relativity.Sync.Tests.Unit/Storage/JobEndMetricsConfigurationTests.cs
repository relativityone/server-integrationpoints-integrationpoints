using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.Storage
{
	internal class JobEndMetricsConfigurationTests : ConfigurationTestBase
	{
		private JobEndMetricsConfiguration _sut;

		private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 102779;
		private const int _SYNC_CONFIGURATION_ARTIFACT_ID = 103799;
		private const int _JOB_HISTORY_TO_RETRY_ARTIFACT_ID = 104799;
        private const int _USER_ID = 323454;

		[SetUp]
		public void SetUp()
		{
			_configurationRdo.JobHistoryToRetryId = _JOB_HISTORY_TO_RETRY_ARTIFACT_ID;

			SyncJobParameters syncJobParameters = new SyncJobParameters(_SYNC_CONFIGURATION_ARTIFACT_ID, _SOURCE_WORKSPACE_ARTIFACT_ID, _USER_ID, It.IsAny<Guid>());
			_sut = new JobEndMetricsConfiguration(_configuration, syncJobParameters);
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

		[TestCase( ImportOverwriteMode.AppendOnly, ImportOverwriteMode.AppendOnly)]
		[TestCase(ImportOverwriteMode.OverlayOnly, ImportOverwriteMode.OverlayOnly)]
		[TestCase(ImportOverwriteMode.AppendOverlay, ImportOverwriteMode.AppendOverlay)]
		public void ImportOverwriteMode_ShouldReturnProperValue(ImportOverwriteMode valueInConfiguration, ImportOverwriteMode expectedValue)
		{
			// Arrange
			_configurationRdo.ImportOverwriteMode = valueInConfiguration;

			// Act
			var importOverwriteMode = _sut.ImportOverwriteMode;

			// Assert
			importOverwriteMode.Should().Be(expectedValue);
		}

		[TestCase(DataSourceType.Production, DataSourceType.Production)]
		[TestCase(DataSourceType.SavedSearch, DataSourceType.SavedSearch)]
		public void DataSourceType_ShouldReturnProperValue(DataSourceType valueInConfiguration, DataSourceType expectedValue)
		{
			// Arrange
			_configurationRdo.DataSourceType = valueInConfiguration;

			// Act
			var dataSourceType = _sut.DataSourceType;

			// Assert
			dataSourceType.Should().Be(expectedValue);
		}

		[TestCase(DestinationLocationType.Folder, DestinationLocationType.Folder)]
		[TestCase(DestinationLocationType.ProductionSet, DestinationLocationType.ProductionSet)]
		public void DestinationType_ShouldReturnProperValue(DestinationLocationType valueInConfiguration, DataSourceType expectedValue)
		{
			// Arrange
			_configurationRdo.DataDestinationType = valueInConfiguration;

			// Act
			DestinationLocationType destinationType = _sut.DestinationType;

			// Assert
			destinationType.Should().Be(expectedValue);
		}

		[TestCase(ImportNativeFileCopyMode.CopyFiles, ImportNativeFileCopyMode.CopyFiles)]
		[TestCase(ImportNativeFileCopyMode.SetFileLinks, ImportNativeFileCopyMode.SetFileLinks)]
		[TestCase(ImportNativeFileCopyMode.DoNotImportNativeFiles, ImportNativeFileCopyMode.DoNotImportNativeFiles)]
		public void ImportNativeFileCopyMode_ShouldReturnProperValue(ImportNativeFileCopyMode valueInConfiguration, ImportNativeFileCopyMode expectedValue)
		{
			// Arrange
			_configurationRdo.NativesBehavior = valueInConfiguration;

			// Act
			var nativeFileCopyMode = _sut.ImportNativeFileCopyMode;

			// Assert
			nativeFileCopyMode.Should().Be(expectedValue);
		}

		[TestCase(ImportImageFileCopyMode.CopyFiles, ImportImageFileCopyMode.CopyFiles)]
		[TestCase(ImportImageFileCopyMode.SetFileLinks, ImportImageFileCopyMode.SetFileLinks)]
		public void ImportImageFileCopyMode_ShouldReturnProperValue(ImportImageFileCopyMode valueInConfiguration, ImportImageFileCopyMode expectedValue)
		{
			// Arrange
			_configurationRdo.ImageFileCopyMode = valueInConfiguration;

			// Act
			var imageFileCopyMode = _sut.ImportImageFileCopyMode;

			// Assert
			imageFileCopyMode.Should().Be(expectedValue);
		}
	}
}
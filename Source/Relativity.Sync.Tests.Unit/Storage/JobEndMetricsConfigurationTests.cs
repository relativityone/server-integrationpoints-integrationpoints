using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Storage;
using Relativity.Sync.Configuration;
using Relativity.Sync.Tests.Common;

namespace Relativity.Sync.Tests.Unit.Storage
{
	internal class JobEndMetricsConfigurationTests : ConfigurationTestBase
	{
		private JobEndMetricsConfiguration _sut;

		private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 102779;
		private const int _SYNC_CONFIGURATION_ARTIFACT_ID = 103799;
		private const int _JOB_HISTORY_TO_RETRY_ARTIFACT_ID = 104799;

		[SetUp]
		public void SetUp()
		{
			_configurationRdo.JobHistoryToRetryId = _JOB_HISTORY_TO_RETRY_ARTIFACT_ID;

			SyncJobParameters syncJobParameters = new SyncJobParameters(_SYNC_CONFIGURATION_ARTIFACT_ID, _SOURCE_WORKSPACE_ARTIFACT_ID, It.IsAny<Guid>());
			_sut = new JobEndMetricsConfiguration(_configuration.Object, syncJobParameters);
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

		[TestCase("AppendOnly", ImportOverwriteMode.AppendOnly)]
		[TestCase("OverlayOnly", ImportOverwriteMode.OverlayOnly)]
		[TestCase("AppendOverlay", ImportOverwriteMode.AppendOverlay)]
		public void ImportOverwriteMode_ShouldReturnProperValue(string valueInConfiguration, ImportOverwriteMode expectedValue)
		{
			// Arrange
			_configurationRdo.ImportOverwriteMode = valueInConfiguration;

			// Act
			var importOverwriteMode = _sut.ImportOverwriteMode;

			// Assert
			importOverwriteMode.Should().Be(expectedValue);
		}

		[TestCase("Production", DataSourceType.Production)]
		[TestCase("SavedSearch", DataSourceType.SavedSearch)]
		public void DataSourceType_ShouldReturnProperValue(string valueInConfiguration, DataSourceType expectedValue)
		{
			// Arrange
			_configurationRdo.DataSourceType = valueInConfiguration;

			// Act
			var dataSourceType = _sut.DataSourceType;

			// Assert
			dataSourceType.Should().Be(expectedValue);
		}

		[TestCase("Folder", DestinationLocationType.Folder)]
		[TestCase("ProductionSet", DestinationLocationType.ProductionSet)]
		public void DestinationType_ShouldReturnProperValue(string valueInConfiguration, DataSourceType expectedValue)
		{
			// Arrange
			_configurationRdo.DataDestinationType = valueInConfiguration;

			// Act
			var destinationType = _sut.DestinationType;

			// Assert
			destinationType.Should().Be(expectedValue);
		}

		[TestCase("Copy", ImportNativeFileCopyMode.CopyFiles)]
		[TestCase("Link", ImportNativeFileCopyMode.SetFileLinks)]
		[TestCase("None", ImportNativeFileCopyMode.DoNotImportNativeFiles)]
		public void ImportNativeFileCopyMode_ShouldReturnProperValue(string valueInConfiguration, ImportNativeFileCopyMode expectedValue)
		{
			// Arrange
			_configurationRdo.NativesBehavior = valueInConfiguration;

			// Act
			var nativeFileCopyMode = _sut.ImportNativeFileCopyMode;

			// Assert
			nativeFileCopyMode.Should().Be(expectedValue);
		}

		[TestCase("Copy", ImportImageFileCopyMode.CopyFiles)]
		[TestCase("Link", ImportImageFileCopyMode.SetFileLinks)]
		public void ImportImageFileCopyMode_ShouldReturnProperValue(string valueInConfiguration, ImportImageFileCopyMode expectedValue)
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
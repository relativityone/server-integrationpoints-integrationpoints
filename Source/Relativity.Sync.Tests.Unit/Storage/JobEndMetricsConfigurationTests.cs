using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Storage;
using System;
using Relativity.Sync.Configuration;
using Relativity.Sync.RDOs;
using IConfiguration = Relativity.Sync.Storage.IConfiguration;

namespace Relativity.Sync.Tests.Unit.Storage
{
	[TestFixture]
	public class JobEndMetricsConfigurationTests
	{
		private Mock<IConfiguration> _configurationFake;
		private JobEndMetricsConfiguration _sut;

		private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 102779;
		private const int _SYNC_CONFIGURATION_ARTIFACT_ID = 103799;
		private const int _JOB_HISTORY_TO_RETRY_ARTIFACT_ID = 104799;

		[SetUp]
		public void SetUp()
		{
			_configurationFake = new Mock<IConfiguration>();
			_configurationFake.Setup(x => x.GetFieldValue<int?>(SyncConfigurationRdo.JobHistoryToRetryIdGuid))
				.Returns(_JOB_HISTORY_TO_RETRY_ARTIFACT_ID);

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

		[TestCase("AppendOnly", ImportOverwriteMode.AppendOnly)]
		[TestCase("OverlayOnly", ImportOverwriteMode.OverlayOnly)]
		[TestCase("AppendOverlay", ImportOverwriteMode.AppendOverlay)]
		public void ImportOverwriteMode_ShouldReturnProperValue(string valueInConfiguration, ImportOverwriteMode expectedValue)
		{
			// Arrange
			_configurationFake.Setup(x => x.GetFieldValue<string>(SyncConfigurationRdo.ImportOverwriteModeGuid))
				.Returns(valueInConfiguration);

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
			_configurationFake.Setup(x => x.GetFieldValue<string>(SyncConfigurationRdo.DataSourceTypeGuid))
				.Returns(valueInConfiguration);

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
			_configurationFake.Setup(x => x.GetFieldValue<string>(SyncConfigurationRdo.DataDestinationTypeGuid))
				.Returns(valueInConfiguration);

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
			_configurationFake.Setup(x => x.GetFieldValue<string>(SyncConfigurationRdo.NativesBehaviorGuid))
				.Returns(valueInConfiguration);

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
			_configurationFake.Setup(x => x.GetFieldValue<string>(SyncConfigurationRdo.ImageFileCopyModeGuid))
				.Returns(valueInConfiguration);

			// Act
			var imageFileCopyMode = _sut.ImportImageFileCopyMode;

			// Assert
			imageFileCopyMode.Should().Be(expectedValue);
		}
	}
}
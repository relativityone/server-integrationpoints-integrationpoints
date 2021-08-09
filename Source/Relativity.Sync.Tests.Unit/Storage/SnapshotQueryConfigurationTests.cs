using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Storage;
using Relativity.Sync.Utils;
using System;

namespace Relativity.Sync.Tests.Unit.Storage
{
	internal class SnapshotQueryConfigurationTests : ConfigurationTestBase
	{
		private JSONSerializer _serializer;

		private SyncJobParameters _syncJobParameters;

		private SnapshotQueryConfiguration _sut;

		[SetUp]
		public void SetUp()
		{
			_serializer = new JSONSerializer();
			_syncJobParameters = new SyncJobParameters(1, 1, Guid.NewGuid());

			_sut = new SnapshotQueryConfiguration(_configuration.Object, _serializer, _syncJobParameters);
		}
		
		[Test]
		public void JobHistoryToRetryId_ShouldReturnValue()
		{
			// Arrange
			const int jobHistoryArtifactId = 104799;
			_configurationRdo.JobHistoryToRetryId = jobHistoryArtifactId;

			// Act & Assert
			_sut.JobHistoryToRetryId.Should().Be(jobHistoryArtifactId);
		}
		
		[Test]
		public void DataSourceArtifactId_ShouldReturnValue()
		{
			// Arrange
			const int dataSourceArtifactId = 105799;
			_configurationRdo.DataSourceArtifactId = dataSourceArtifactId;

			// Act & Assert
			_sut.DataSourceArtifactId.Should().Be(dataSourceArtifactId);
		}

		[Test]
		public void WorkspaceId_ShouldReturnValue()
		{
			// Act & Assert
			_sut.SourceWorkspaceArtifactId.Should().Be(1);
		}

		[Test]
		public void ProductionImagePrecedence_ShouldReturnValue()
		{
			// Arrange
			var expectedValue = new[] { 1, 2, 3 };
			_configurationRdo.ProductionImagePrecedence = _serializer.Serialize(expectedValue);

			// Act & Assert
			_sut.ProductionImagePrecedence.Should().BeEquivalentTo(expectedValue);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void IncludeOriginalImageIfNotFoundInProductions_ShouldReturnValue(bool expectedValue)
		{
			// Arrange
			_configurationRdo.IncludeOriginalImages = expectedValue;

			// Act & Assert
			_sut.IncludeOriginalImageIfNotFoundInProductions.Should().Be(expectedValue);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.RDOs;
using Relativity.Sync.Storage;
using IConfiguration = Relativity.Sync.Storage.IConfiguration;

namespace Relativity.Sync.Tests.Unit.Storage
{

	[TestFixture]
	public sealed class DocumentRetryDataSourceSnapshotConfigurationTests
	{
		private DocumentRetryDataSourceSnapshotConfiguration _instance;

		private Mock<IConfiguration> _cache;
		private Mock<IFieldMappings> _fieldMappings;

		private const int _WORKSPACE_ID = 589632;

		[SetUp]
		public void SetUp()
		{
			_cache = new Mock<IConfiguration>();
			_fieldMappings = new Mock<IFieldMappings>();

			_instance = new DocumentRetryDataSourceSnapshotConfiguration(_cache.Object, _fieldMappings.Object, new SyncJobParameters(1, _WORKSPACE_ID, 1));
		}

		[Test]
		public void ItShouldRetrieveSourceWorkspaceArtifactId()
		{
			// Act & Assert
			_instance.SourceWorkspaceArtifactId.Should().Be(_WORKSPACE_ID);
		}

		[Test]
		public void ItShouldRetrieveDataSourceArtifactId()
		{
			// Arrange
			const int expectedValue = 658932;

			_cache.Setup(x => x.GetFieldValue<int>(SyncConfigurationRdo.DataSourceArtifactIdGuid)).Returns(expectedValue);

			// Act & Assert
			_instance.DataSourceArtifactId.Should().Be(expectedValue);
		}

		[Test]
		public void ItShouldRetrieveFieldMappings()
		{
			// Arrange
			List<FieldMap> fieldMappings = new List<FieldMap>();
			_fieldMappings.Setup(x => x.GetFieldMappings()).Returns(fieldMappings);

			// Act & Assert
			_instance.GetFieldMappings().Should().BeSameAs(fieldMappings);
		}

		[Test]
		[TestCase("", false)]
		[TestCase(null, false)]
		[TestCase("guid", true)]
		public void ItShouldRetrieveIsSnapshotCreated(string snapshot, bool expectedValue)
		{
			// Arrange
			_cache.Setup(x => x.GetFieldValue<string>(SyncConfigurationRdo.SnapshotIdGuid)).Returns(snapshot);

			// Act & Assert
			_instance.IsSnapshotCreated.Should().Be(expectedValue);
		}

		[Test]
		public async Task ItShouldUpdateSnapshotData()
		{
			// Arrange
			Guid snapshotId = Guid.NewGuid();
			const int totalRecordsCount = 789654;

			// Act
			await _instance.SetSnapshotDataAsync(snapshotId, totalRecordsCount).ConfigureAwait(false);

			// Assert
			_cache.Verify(x => x.UpdateFieldValueAsync(SyncConfigurationRdo.SnapshotIdGuid, snapshotId.ToString()));
			_cache.Verify(x => x.UpdateFieldValueAsync(SyncConfigurationRdo.SnapshotRecordsCountGuid, totalRecordsCount));
		}

		[Test]
		public void ItShouldRetrieveJobHistoryToRetryID_WhenNotNull()
		{
			// Arrange
			const int expectedValue = 1;

			_cache.Setup(x => x.GetFieldValue<RelativityObjectValue>(SyncConfigurationRdo.JobHistoryToRetryGuid)).Returns(new RelativityObjectValue{ArtifactID = expectedValue});

			// Act & Assert
			_instance.JobHistoryToRetryId.Should().Be(expectedValue);
		}

		[Test]
		public void ItShouldRetrieveJobHistoryToRetryID_WhenNull()
		{
			// Arrange
			_cache.Setup(x => x.GetFieldValue<RelativityObjectValue>(SyncConfigurationRdo.JobHistoryToRetryGuid)).Returns((RelativityObjectValue)null);

			// Act & Assert
			_instance.JobHistoryToRetryId.Should().Be(null);
		}
	}
}
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

		private static readonly Guid SnapshotIdGuid = new Guid("D1210A1B-C461-46CB-9B73-9D22D05880C5");
		private static readonly Guid JobHistoryToRetryGuid = new Guid("d7d0ddb9-d383-4578-8d7b-6cbdd9e71549");
		private static readonly Guid SnapshotRecordsCountGuid = new Guid("57B93F20-2648-4ACF-973B-BCBA8A08E2BD");

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
			_cache.Setup(x => x.GetFieldValue<string>(SnapshotIdGuid)).Returns(snapshot);

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
			_cache.Verify(x => x.UpdateFieldValueAsync(SnapshotIdGuid, snapshotId.ToString()));
			_cache.Verify(x => x.UpdateFieldValueAsync(SnapshotRecordsCountGuid, totalRecordsCount));
		}

		[Test]
		public void ItShouldRetrieveJobHistoryToRetryID_WhenNotNull()
		{
			// Arrange
			const int expectedValue = 1;

			_cache.Setup(x => x.GetFieldValue<RelativityObjectValue>(JobHistoryToRetryGuid)).Returns(new RelativityObjectValue{ArtifactID = expectedValue});

			// Act & Assert
			_instance.JobHistoryToRetryId.Should().Be(expectedValue);
		}

		[Test]
		public void ItShouldRetrieveJobHistoryToRetryID_WhenNull()
		{
			// Arrange
			_cache.Setup(x => x.GetFieldValue<RelativityObjectValue>(JobHistoryToRetryGuid)).Returns((RelativityObjectValue)null);

			// Act & Assert
			_instance.JobHistoryToRetryId.Should().Be(null);
		}
	}
}
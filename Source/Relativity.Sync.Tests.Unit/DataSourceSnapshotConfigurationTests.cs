using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using IConfiguration = Relativity.Sync.Storage.IConfiguration;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public sealed class DataSourceSnapshotConfigurationTests
	{
		private DataSourceSnapshotConfiguration _instance;

		private Mock<IConfiguration> _cache;
		private Mock<IFieldMappings> _fieldMappings;

		private const int _WORKSPACE_ID = 589632;

		private static readonly Guid DataSourceArtifactIdGuid = new Guid("6D8631F9-0EA1-4EB9-B7B2-C552F43959D0");
		private static readonly Guid SnapshotIdGuid = new Guid("D1210A1B-C461-46CB-9B73-9D22D05880C5");
		private static readonly Guid SnapshotRecordsCountGuid = new Guid("57B93F20-2648-4ACF-973B-BCBA8A08E2BD");
		private static readonly Guid DestinationFolderStructureBehaviorGuid = new Guid("A1593105-BD99-4A15-A51A-3AA8D4195908");
		private static readonly Guid FolderPathSourceFieldArtifactIdGuid = new Guid("BF5F07A3-6349-47EE-9618-1DD32C9FD998");

		[SetUp]
		public void SetUp()
		{
			_cache = new Mock<IConfiguration>();
			_fieldMappings = new Mock<IFieldMappings>();

			_instance = new DataSourceSnapshotConfiguration(_cache.Object, _fieldMappings.Object, new SyncJobParameters(1, _WORKSPACE_ID));
		}

		[Test]
		public void ItShouldRetrieveSourceWorkspaceArtifactId()
		{
			_instance.SourceWorkspaceArtifactId.Should().Be(_WORKSPACE_ID);
		}

		[Test]
		public void ItShouldRetrieveDataSourceArtifactId()
		{
			const int expectedValue = 658932;

			_cache.Setup(x => x.GetFieldValue<int>(DataSourceArtifactIdGuid)).Returns(expectedValue);

			_instance.DataSourceArtifactId.Should().Be(expectedValue);
		}

		[Test]
		public void ItShouldRetrieveFieldMappings()
		{
			List<FieldMap> fieldMappings = new List<FieldMap>();
			_fieldMappings.Setup(x => x.GetFieldMappings()).Returns(fieldMappings);
			
			_instance.FieldMappings.Should().BeSameAs(fieldMappings);
		}

		[Test]
		public void ItShouldRetrieveDestinationFolderStructureBehavior()
		{
			DestinationFolderStructureBehavior expectedValue = DestinationFolderStructureBehavior.RetainSourceWorkspaceStructure;

			_cache.Setup(x => x.GetFieldValue<string>(DestinationFolderStructureBehaviorGuid)).Returns(expectedValue.ToString);

			_instance.DestinationFolderStructureBehavior.Should().Be(expectedValue);
		}

		[Test]
		public void ItShouldRetrieveFolderPathSourceFieldArtifactId()
		{
			const int expectedValue = 845967;

			_cache.Setup(x => x.GetFieldValue<int>(FolderPathSourceFieldArtifactIdGuid)).Returns(expectedValue);

			_instance.FolderPathSourceFieldArtifactId.Should().Be(expectedValue);
		}

		[Test]
		[TestCase("", false)]
		[TestCase(null, false)]
		[TestCase("guid", true)]
		public void ItShouldRetrieveIsSnapshotCreated(string snapshot, bool expectedValue)
		{
			_cache.Setup(x => x.GetFieldValue<string>(SnapshotIdGuid)).Returns(snapshot);

			_instance.IsSnapshotCreated.Should().Be(expectedValue);
		}

		[Test]
		public async Task ItShouldUpdateSnapshotData()
		{
			Guid snapshotId = Guid.NewGuid();
			const long totalRecordsCount = 789654;

			await _instance.SetSnapshotDataAsync(snapshotId, totalRecordsCount).ConfigureAwait(false);

			_cache.Verify(x => x.UpdateFieldValueAsync(SnapshotIdGuid, snapshotId.ToString()));
			_cache.Verify(x => x.UpdateFieldValueAsync(SnapshotRecordsCountGuid, totalRecordsCount));
		}
	}
}
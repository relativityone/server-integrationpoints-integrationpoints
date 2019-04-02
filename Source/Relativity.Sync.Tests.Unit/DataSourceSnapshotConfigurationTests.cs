using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public sealed class DataSourceSnapshotConfigurationTests
	{
		private DataSourceSnapshotConfiguration _instance;

		private Mock<IConfigurationCache> _cache;

		private const int _WORKSPACE_ID = 589632;

		private static readonly Guid DataSourceArtifactIdGuid = new Guid("6D8631F9-0EA1-4EB9-B7B2-C552F43959D0");
		private static readonly Guid FieldMappingsGuid = new Guid("E3CB5C64-C726-47F8-9CB0-1391C5911628");
		private static readonly Guid SnapshotIdGuid = new Guid("D1210A1B-C461-46CB-9B73-9D22D05880C5");
		private static readonly Guid SnapshotRecordsCountGuid = new Guid("57B93F20-2648-4ACF-973B-BCBA8A08E2BD");

		[SetUp]
		public void SetUp()
		{
			_cache = new Mock<IConfigurationCache>();

			_instance = new DataSourceSnapshotConfiguration(_cache.Object, _WORKSPACE_ID);
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
			const string expectedValue = "fields";

			_cache.Setup(x => x.GetFieldValue<string>(FieldMappingsGuid)).Returns(expectedValue);

			_instance.FieldMappings.Should().Be(expectedValue);
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
			const int totalRecordsCount = 789654;

			await _instance.SetSnapshotDataAsync(snapshotId, totalRecordsCount).ConfigureAwait(false);

			_cache.Verify(x => x.UpdateFieldValueAsync(SnapshotIdGuid, snapshotId.ToString()));
			_cache.Verify(x => x.UpdateFieldValueAsync(SnapshotRecordsCountGuid, totalRecordsCount));
		}
	}
}
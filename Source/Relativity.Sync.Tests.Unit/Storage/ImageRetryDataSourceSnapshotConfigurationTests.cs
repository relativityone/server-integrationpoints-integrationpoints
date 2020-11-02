using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Storage;
using Relativity.Sync.Utils;
using IConfiguration = Relativity.Sync.Storage.IConfiguration;

namespace Relativity.Sync.Tests.Unit.Storage
{

	[TestFixture]
	public sealed class ImageRetryDataSourceSnapshotConfigurationTests
	{
		private ImageRetryDataSourceSnapshotConfiguration _instance;

		private Mock<IConfiguration> _cache;
		private JSONSerializer _serializer;


		private const int _WORKSPACE_ID = 589632;

		private static readonly Guid DataSourceArtifactIdGuid = new Guid("6D8631F9-0EA1-4EB9-B7B2-C552F43959D0");
		private static readonly Guid SnapshotIdGuid = new Guid("D1210A1B-C461-46CB-9B73-9D22D05880C5");
		private static readonly Guid JobHistoryToRetryGuid = new Guid("d7d0ddb9-d383-4578-8d7b-6cbdd9e71549");
		private static readonly Guid SnapshotRecordsCountGuid = new Guid("57B93F20-2648-4ACF-973B-BCBA8A08E2BD");
		private static readonly Guid IncludeOriginalImagesGuid = new Guid("f2cad5c5-63d5-49fc-bd47-885661ef1d8b");
		private static readonly Guid ProductionImagePrecedenceGuid = new Guid("421cf05e-bab4-4455-a9ca-fa83d686b5ed");

		[SetUp]
		public void SetUp()
		{
			_cache = new Mock<IConfiguration>();
			_serializer = new JSONSerializer();

			_instance = new ImageRetryDataSourceSnapshotConfiguration(_cache.Object, _serializer, new SyncJobParameters(1, _WORKSPACE_ID, 1));
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

			_cache.Setup(x => x.GetFieldValue<int>(DataSourceArtifactIdGuid)).Returns(expectedValue);
			
			// Act & Assert
			_instance.DataSourceArtifactId.Should().Be(expectedValue);
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

		[Test]
		public void ProductionIds__ShouldBeRetrieved()
		{
			// Arrange
			var expectedValue = new[] { 1, 2, 3 };
			_cache.Setup(x => x.GetFieldValue<string>(ProductionImagePrecedenceGuid)).Returns(_serializer.Serialize(expectedValue));

			// Act & Assert
			_instance.ProductionImagePrecedence.Should().BeEquivalentTo(expectedValue);
		}

		[TestCase(new[] { 1, 2, 3 }, true)]
		[TestCase(new int[] { }, false)]
		public void IsProductionImagePrecedenceSet_ShouldReturnValue_BasedOnProductionIds(int[] productionIds, bool expectedProductionIsSetValue)
		{
			// Arrange
			_cache.Setup(x => x.GetFieldValue<string>(ProductionImagePrecedenceGuid)).Returns(_serializer.Serialize(productionIds));

			// Act & Assert
			_instance.IsProductionImagePrecedenceSet.Should().Be(expectedProductionIsSetValue);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void IncludeOriginalImageIfNotFoundInProductions_ShouldBeRetrieved(bool expectedValue)
		{
			// Arrange
			_cache.Setup(x => x.GetFieldValue<bool>(IncludeOriginalImagesGuid)).Returns(expectedValue);

			// Act & Assert
			_instance.IncludeOriginalImageIfNotFoundInProductions.Should().Be(expectedValue);
		}
	}
}
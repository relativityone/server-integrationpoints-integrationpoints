﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.RDOs;
using Relativity.Sync.Storage;
using Relativity.Sync.Utils;
using IConfiguration = Relativity.Sync.Storage.IConfiguration;

namespace Relativity.Sync.Tests.Unit.Storage
{

	[TestFixture]
	public sealed class ImageDataSourceSnapshotConfigurationTests
	{
		private ImageDataSourceSnapshotConfiguration _instance;

		private Mock<IConfiguration> _cache;

		private const int _WORKSPACE_ID = 589632;

		private static readonly Guid SnapshotIdGuid = new Guid("D1210A1B-C461-46CB-9B73-9D22D05880C5");
		private static readonly Guid SnapshotRecordsCountGuid = new Guid("57B93F20-2648-4ACF-973B-BCBA8A08E2BD");
		private ISerializer _serializer;

		[SetUp]
		public void SetUp()
		{
			_cache = new Mock<IConfiguration>();
			_serializer = new JSONSerializer();

			_instance = new ImageDataSourceSnapshotConfiguration(_cache.Object, _serializer, new SyncJobParameters(1, _WORKSPACE_ID, 1));
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
		public void ProductionIds__ShouldBeRetrieved()
		{
			// Arrange
			var expectedValue = new [] {1, 2, 3};
			_cache.Setup(x => x.GetFieldValue<string>(SyncConfigurationRdo.ProductionImagePrecedenceGuid)).Returns(_serializer.Serialize(expectedValue));

			// Act & Assert
			_instance.ProductionImagePrecedence.Should().BeEquivalentTo(expectedValue);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void IncludeOriginalImageIfNotFoundInProductions_ShouldBeRetrieved(bool expectedValue)
		{
			// Arrange
			_cache.Setup(x => x.GetFieldValue<bool>(SyncConfigurationRdo.IncludeOriginalImagesGuid)).Returns(expectedValue);

			// Act & Assert
			_instance.IncludeOriginalImageIfNotFoundInProductions.Should().Be(expectedValue);
		}
	}
}
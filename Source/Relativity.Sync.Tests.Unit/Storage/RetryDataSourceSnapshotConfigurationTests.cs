﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.Storage
{
    [TestFixture]
    internal sealed class RetryDataSourceSnapshotConfigurationTests : ConfigurationTestBase
    {
        private RetryDataSourceSnapshotConfiguration _instance;

        private Mock<IFieldMappings> _fieldMappings;

        private const int _WORKSPACE_ID = 589632;
        private const int _USER_ID = 323454;

        [SetUp]
        public void SetUp()
        {
            _fieldMappings = new Mock<IFieldMappings>();

            SyncJobParameters parameters = new SyncJobParameters(It.IsAny<int>(), _WORKSPACE_ID, _USER_ID, It.IsAny<Guid>(), Guid.Empty);

            _instance = new RetryDataSourceSnapshotConfiguration(_configuration, _fieldMappings.Object, parameters);
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

            _configurationRdo.DataSourceArtifactId = expectedValue;

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
        [TestCaseSource(nameof(SnapshotCaseSource))]
        public void ItShouldRetrieveIsSnapshotCreated(Guid? snapshot, bool expectedValue)
        {
            // Arrange
            _configurationRdo.SnapshotId = snapshot;

            // Act & Assert
            _instance.IsSnapshotCreated.Should().Be(expectedValue);
        }

        private static IEnumerable<TestCaseData> SnapshotCaseSource()
        {
            yield return new TestCaseData((Guid?)null, false);
            yield return new TestCaseData((Guid?)Guid.Empty, false);
            yield return new TestCaseData((Guid?)Guid.NewGuid(), true);
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
            _configurationRdo.SnapshotId.Should().Be(snapshotId);
            _configurationRdo.SnapshotRecordsCount.Should().Be(totalRecordsCount);
        }

        [Test]
        public void ItShouldRetrieveJobHistoryToRetryID_WhenNotNull()
        {
            // Arrange
            const int expectedValue = 1;

            _configurationRdo.JobHistoryToRetryId = expectedValue;

            // Act & Assert
            _instance.JobHistoryToRetryId.Should().Be(expectedValue);
        }

        [Test]
        public void ItShouldRetrieveJobHistoryToRetryID_WhenNull()
        {
            // Arrange
            _configurationRdo.JobHistoryToRetryId = null;

            // Act & Assert
            _instance.JobHistoryToRetryId.Should().Be(null);
        }
    }
}

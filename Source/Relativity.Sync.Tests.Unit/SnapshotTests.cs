using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Executors;

namespace Relativity.Sync.Tests.Unit
{
#pragma warning disable RG2009 // Hardcoded Numeric Value
    [TestFixture]
    internal static class SnapshotTests
    {
        [Test]
        public static void ItShouldReturnTotalNumberOfRecords()
        {
            const int total = 852369;

            // ACT
            Snapshot snapshot = new Snapshot(total, 1, 0);

            // ASSERT
            snapshot.TotalRecordsCount.Should().Be(total);
        }

        [Test]
        public static void ItShouldReturnBatchSize()
        {
            const int batchSize = 784596;

            // ACT
            Snapshot snapshot = new Snapshot(1, batchSize, 0);

            // ASSERT
            snapshot.BatchSize.Should().Be(batchSize);
        }

        [Test]
        [TestCase(1, 1, 1)]
        [TestCase(2, 1, 2)]
        [TestCase(1000, 100, 10)]
        [TestCase(99, 10, 10)]
        [TestCase(91, 10, 10)]
        [TestCase(100, 1000, 1)]
        public static void ItShouldCalculateNumberOfBatchesToCreate(int recordsCount, int batchSize, int expectedNumberOfBatches)
        {
            // ACT
            Snapshot snapshot = new Snapshot(recordsCount, batchSize, 0);

            // ASSERT
            snapshot.TotalNumberOfBatchesToCreate.Should().Be(expectedNumberOfBatches);
            snapshot.GetSnapshotParts().Count.Should().Be(expectedNumberOfBatches);
        }

        [Test]
        [TestCase(1000, 0)]
        [TestCase(1001, 0)]
        [TestCase(999, 1)]
        [TestCase(900, 1)]
        [TestCase(899, 2)]
        [TestCase(555, 5)]
        public static void ItShouldCalculateNumberOfBatchesToCreateIncorporatingNumberOfRecords(int numberOfRecordsIncludedInBatch, int expectedNumberOfBatches)
        {
            const int recordsCount = 1000;
            const int batchSize = 100;

            // ACT
            Snapshot snapshot = new Snapshot(recordsCount, batchSize, numberOfRecordsIncludedInBatch);

            // ASSERT
            snapshot.TotalNumberOfBatchesToCreate.Should().Be(expectedNumberOfBatches);
            snapshot.GetSnapshotParts().Count.Should().Be(expectedNumberOfBatches);
        }

        [Test]
        public static void ItShouldHandleEdgeCaseWhenCalculatingNumberOfBatches()
        {
            // ACT
            Snapshot snapshot = new Snapshot(int.MaxValue, int.MaxValue, 0);

            // ASSERT
            snapshot.TotalNumberOfBatchesToCreate.Should().Be(1);
        }

        [Test]
        public static void ItShouldCreateSnapshotParts()
        {
            const int batchSize = 10;
            const int total = 25;

            Snapshot snapshot = new Snapshot(total, batchSize, 0);

            // ACT
            List<SnapshotPart> parts = snapshot.GetSnapshotParts();

            // ASSERT
            const int expectedNumberOfParts = 3;
            parts.Count.Should().Be(expectedNumberOfParts);

            // first batch
            parts[0].StartingIndex.Should().Be(0);
            parts[0].NumberOfRecords.Should().Be(batchSize);

            // second batch
            parts[1].StartingIndex.Should().Be(batchSize);
            parts[1].NumberOfRecords.Should().Be(batchSize);

            // third batch
            parts[2].StartingIndex.Should().Be(batchSize * 2);
            const int recordsLeftInLastBatch = 5;
            parts[2].NumberOfRecords.Should().Be(recordsLeftInLastBatch);
        }

        [Test]
        public static void ItShouldCreateSnapshotPartsIncorporatingNumberOfRecords()
        {
            const int batchSize = 10;
            const int total = 25;
            const int numberOfRecords = 12;

            Snapshot snapshot = new Snapshot(total, batchSize, numberOfRecords);

            // ACT
            List<SnapshotPart> parts = snapshot.GetSnapshotParts();

            // ASSERT
            const int expectedNumberOfParts = 2;
            parts.Count.Should().Be(expectedNumberOfParts);

            // first batch
            parts[0].StartingIndex.Should().Be(numberOfRecords);
            parts[0].NumberOfRecords.Should().Be(batchSize);

            // second batch
            parts[1].StartingIndex.Should().Be(numberOfRecords + batchSize);
            const int recordsLeftInLastBatch = 3;
            parts[1].NumberOfRecords.Should().Be(recordsLeftInLastBatch);
        }

        [Test]
        public static void ItShouldCacheParts()
        {
            Snapshot snapshot = new Snapshot(1, 1, 0);

            // ACT
            List<SnapshotPart> parts1 = snapshot.GetSnapshotParts();
            List<SnapshotPart> parts2 = snapshot.GetSnapshotParts();

            // ASSERT
            parts1.Should().BeSameAs(parts2);
        }
    }
#pragma warning restore RG2009 // Hardcoded Numeric Value
}
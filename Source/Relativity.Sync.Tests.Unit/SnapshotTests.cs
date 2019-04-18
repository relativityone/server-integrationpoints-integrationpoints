using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Executors;

namespace Relativity.Sync.Tests.Unit
{
#pragma warning disable RG2009 // Hardcoded Numeric Value
	[TestFixture]
	internal sealed class SnapshotTests
	{
		[Test]
		public static void ItShouldReturnTotalNumberOfRecords()
		{
			const int total = 852369;

			Snapshot snapshot = new Snapshot(total, 1);

			snapshot.TotalRecordsCount.Should().Be(total);
		}

		[Test]
		public static void ItShouldReturnBatchSize()
		{
			const int batchSize = 784596;

			Snapshot snapshot = new Snapshot(1, batchSize);

			snapshot.BatchSize.Should().Be(batchSize);
		}

		[Test]
		[TestCase(1, 1, 1)]
		[TestCase(2, 1, 2)]
		[TestCase(1000, 100, 10)]
		[TestCase(99, 10, 10)]
		[TestCase(91, 10, 10)]
		[TestCase(100, 1000, 1)]
		public static void ItShouldCalculateNumberOfBatches(int recordsCount, int batchSize, int expectedNumberOfBatches)
		{
			Snapshot snapshot = new Snapshot(recordsCount, batchSize);

			snapshot.TotalNumberOfBatches.Should().Be(expectedNumberOfBatches);
			snapshot.GetSnapshotParts().Count.Should().Be(expectedNumberOfBatches);
		}

		[Test]
		public static void ItShouldHandleEdgeCaseWhenCalculatingNumberOfBatches()
		{
			Snapshot snapshot = new Snapshot(int.MaxValue, int.MaxValue);

			snapshot.TotalNumberOfBatches.Should().Be(1);
		}

		[Test]
		public static void ItShouldCreateSnapshotParts()
		{
			const int batchSize = 10;
			const int total = 25;

			Snapshot snapshot = new Snapshot(total, batchSize);

			List<SnapshotPart> parts = snapshot.GetSnapshotParts();

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
		public static void ItShouldCacheParts()
		{
			Snapshot snapshot = new Snapshot(1, 1);

			List<SnapshotPart> parts1 = snapshot.GetSnapshotParts();
			List<SnapshotPart> parts2 = snapshot.GetSnapshotParts();

			parts1.Should().BeSameAs(parts2);
		}
	}
#pragma warning restore RG2009 // Hardcoded Numeric Value
}
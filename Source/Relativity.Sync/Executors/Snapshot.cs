using System;
using System.Collections.Generic;

namespace Relativity.Sync.Executors
{
	internal sealed class Snapshot
	{
		private List<SnapshotPart> _parts;

		private readonly int _numberOfRecordsIncludedInBatch;

		public Snapshot(int totalRecordsCount, int batchSize, int numberOfRecordsIncludedInBatch)
		{
			_numberOfRecordsIncludedInBatch = numberOfRecordsIncludedInBatch;
			TotalRecordsCount = totalRecordsCount;
			BatchSize = batchSize;
		}

		public int TotalRecordsCount { get; }

		public int TotalNumberOfBatchesToCreate
		{
			get
			{
				if (TotalRecordsCount <= _numberOfRecordsIncludedInBatch)
				{
					return 0;
				}

				return (TotalRecordsCount - _numberOfRecordsIncludedInBatch - 1) / BatchSize + 1;
			}
		}

		public int BatchSize { get; }

		public List<SnapshotPart> GetSnapshotParts()
		{
			if (_parts != null)
			{
				return _parts;
			}

			_parts = new List<SnapshotPart>();
			for (int i = 0; i < TotalNumberOfBatchesToCreate; i++)
			{
				int currentStartingIndex = i * BatchSize + _numberOfRecordsIncludedInBatch;
				int currentNumberOfRecords = Math.Min(BatchSize, TotalRecordsCount - currentStartingIndex);
				_parts.Add(new SnapshotPart(currentStartingIndex, currentNumberOfRecords));
			}

			return _parts;
		}
	}
}
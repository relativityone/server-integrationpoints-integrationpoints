using System;
using System.Collections.Generic;

namespace Relativity.Sync.Executors
{
	internal sealed class Snapshot
	{
		private List<SnapshotPart> _parts;

		public Snapshot(int totalRecordsCount, SyncJobExecutionConfiguration configuration)
		{
			TotalRecordsCount = totalRecordsCount;
			BatchSize = configuration.BatchSize;
		}

		public int TotalRecordsCount { get; }

		public int TotalNumberOfBatches => (TotalRecordsCount - 1) / BatchSize + 1;

		public int BatchSize { get; }

		public List<SnapshotPart> GetSnapshotParts()
		{
			if (_parts != null)
			{
				return _parts;
			}

			_parts = new List<SnapshotPart>();
			for (int i = 0; i < TotalNumberOfBatches; i++)
			{
				int currentStartingIndex = i * BatchSize;
				int currentNumberOfRecords = Math.Min(BatchSize, TotalRecordsCount - currentStartingIndex);
				_parts.Add(new SnapshotPart(currentStartingIndex, currentNumberOfRecords));
			}

			return _parts;
		}
	}
}
using System;
using System.Collections.Generic;

namespace Relativity.Sync.Configuration
{
	internal interface ISnapshotPartitionConfiguration : IConfiguration
	{
		int TotalRecordsCount { get; }

		Guid ExportRunId { get; }

		bool IsSnapshotPartitioned { get; }

		void SetSnapshotPartitions(List<int> batchesIds);
	}
}
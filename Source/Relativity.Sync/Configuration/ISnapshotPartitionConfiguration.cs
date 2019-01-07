using System.Collections.Generic;

namespace Relativity.Sync.Configuration
{
	internal interface ISnapshotPartitionConfiguration : IConfiguration
	{
		int SnapshotId { get; }

		bool AreBatchesIdsSet { get; }

		List<int> BatchesIds { set; }
	}
}
using Relativity.Sync.RDOs.Framework;
using Relativity.Sync.RDOs.Framework.Attributes;

namespace Relativity.Sync.RDOs
{
	[Rdo(SyncBatchGuids.SyncBatchObjectTypeGuid, "Sync Batch", SyncRdoGuids.SyncConfigurationGuid)]
	internal sealed class SyncBatchRdo : IRdoType
	{
		public int ArtifactId { get; set; }

		[RdoField(SyncBatchGuids.FailedItemsCountGuid, RdoFieldType.WholeNumber)]
		public int FailedItemsCount { get; set; }

		[RdoField(SyncBatchGuids.LockedByGuid, RdoFieldType.FixedLengthText)]
		public string LockedBy { get; set; }

		[RdoField(SyncBatchGuids.ProgressGuid, RdoFieldType.Decimal)]
		public double Progress { get; set; }

		[RdoField(SyncBatchGuids.StartingIndexGuid, RdoFieldType.WholeNumber)]
		public int StartingIndex { get; set; }

		[RdoField(SyncBatchGuids.StatusGuid, RdoFieldType.FixedLengthText)]
		public string Status { get; set; }

		[RdoField(SyncBatchGuids.TransferredItemsCountGuid, RdoFieldType.WholeNumber)]
		public int TransferredItemsCount { get; set; }

		[RdoField(SyncBatchGuids.TaggedItemsCountGuid, RdoFieldType.WholeNumber)]
		public int TaggedItemsCount { get; set; }

		[RdoField(SyncBatchGuids.TotalItemsCountGuid, RdoFieldType.WholeNumber)]
		public int TotalItemsCount { get; set; }
	}
}
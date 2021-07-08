using Relativity.Sync.RDOs.Framework;
using Relativity.Sync.RDOs.Framework.Attributes;

namespace Relativity.Sync.RDOs
{
	[Rdo(SyncBatchGuids.SyncBatchObjectTypeGuid, "Sync Batch", SyncRdoGuids.SyncConfigurationGuid)]
	internal sealed class SyncBatchRdo : IRdoType
	{
		public int ArtifactId { get; set; }
		
		[RdoField(SyncBatchGuids.StartingIndexGuid, RdoFieldType.WholeNumber)]
		public int StartingIndex { get; set; }

		[RdoField(SyncBatchGuids.StatusGuid, RdoFieldType.FixedLengthText)]
		public string Status { get; set; }

		[RdoField(SyncBatchGuids.TaggedDocumentsCountGuid, RdoFieldType.WholeNumber)]
		public int TaggedDocumentsCount { get; set; }


		[RdoField(SyncBatchGuids.TotalItemsCountGuid, RdoFieldType.WholeNumber)]
		public int TotalItemsCount { get; set; }

		[RdoField(SyncBatchGuids.TransferredItemsCountGuid, RdoFieldType.WholeNumber)]
		public int TransferredItemsCount { get; set; }

		[RdoField(SyncBatchGuids.FailedItemsCountGuid, RdoFieldType.WholeNumber)]
		public int FailedItemsCount { get; set; }


		[RdoField(SyncBatchGuids.TotalDocumentsCountGuid, RdoFieldType.WholeNumber)]
		public int TotalDocumentsCount { get; set; }

		[RdoField(SyncBatchGuids.TransferredDocumentsCountGuid, RdoFieldType.WholeNumber)]
		public int TransferredDocumentsCount { get; set; }

		[RdoField(SyncBatchGuids.FailedDocumentsCountGuid, RdoFieldType.WholeNumber)]
		public int FailedDocumentsCount { get; set; }
	}
}
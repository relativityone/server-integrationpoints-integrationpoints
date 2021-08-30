using System;
using Relativity.Sync.RDOs.Framework;
using Relativity.Sync.RDOs.Framework.Attributes;

namespace Relativity.Sync.RDOs
{
	[Rdo(SyncStatisticsGuids.SyncStatisticsGuid, "Sync Statistics")]
	internal sealed class SyncStatisticsRdo : IRdoType
	{
		public int ArtifactId { get; set; }

		[RdoLongField(SyncStatisticsGuids.DocumentsCalculatedGuid)]
		public long DocumentsCalculated { get; set; }

		[RdoLongField(SyncStatisticsGuids.DocumentsRequestedGuid)]
		public long DocumentsRequested { get; set; }

		[RdoLongField(SyncStatisticsGuids.FilesSizeCalculated)]
		public long FilesSizeCalculated { get; set; }

		[RdoLongField(SyncStatisticsGuids.FilesCountCalculated)]
		public long FilesCountCalculated { get; set; }

		[RdoField(SyncStatisticsGuids.RunIdGuid, RdoFieldType.FixedLengthText, fixedTextLength: 36)]
		public Guid RunId { get; set; }
	}
}

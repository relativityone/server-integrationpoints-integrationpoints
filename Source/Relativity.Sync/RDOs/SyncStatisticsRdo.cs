using System;
using Relativity.Sync.RDOs.Framework;
using Relativity.Sync.RDOs.Framework.Attributes;

namespace Relativity.Sync.RDOs
{
	[Rdo(SyncStatisticsGuids.SyncStatisticsGuid, "Sync Statistics")]
	internal sealed class SyncStatisticsRdo : IRdoType
	{
		public int ArtifactId { get; set; }

		[RdoField(SyncStatisticsGuids.DocumentsCalculatedGuid, RdoFieldType.WholeNumber)]
		public long DocumentsCalculated { get; set; }

		[RdoField(SyncStatisticsGuids.DocumentsRequestedGuid, RdoFieldType.WholeNumber)]
		public long DocumentsRequested { get; set; }

		[RdoField(SyncStatisticsGuids.FilesSizeCalculated, RdoFieldType.WholeNumber)]
		public long FilesSizeCalculated { get; set; }

		[RdoField(SyncStatisticsGuids.FilesCountCalculated, RdoFieldType.WholeNumber)]
		public long FilesCountCalculated { get; set; }

		[RdoField(SyncStatisticsGuids.RunIdGuid, RdoFieldType.FixedLengthText, fixedTextLength: 36)]
		public Guid RunId { get; set; }
	}
}

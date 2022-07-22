using System;
using Relativity.Sync.RDOs.Framework;
using Relativity.Sync.RDOs.Framework.Attributes;

namespace Relativity.Sync.RDOs
{
    [Rdo(SyncStatisticsGuids.SyncStatisticsGuid, "Sync Statistics")]
    internal sealed class SyncStatisticsRdo : IRdoType
    {
        public int ArtifactId { get; set; }

        [RdoLongField(SyncStatisticsGuids.CalculatedDocumentsGuid)]
        public long CalculatedDocuments { get; set; }

        [RdoLongField(SyncStatisticsGuids.RequestedDocumentsGuid)]
        public long RequestedDocuments { get; set; }

        [RdoLongField(SyncStatisticsGuids.CalculatedFilesSizeGuid)]
        public long CalculatedFilesSize { get; set; }

        [RdoLongField(SyncStatisticsGuids.CalculatedFilesCountGuid)]
        public long CalculatedFilesCount { get; set; }

        [RdoField(SyncStatisticsGuids.RunIdGuid, RdoFieldType.FixedLengthText, fixedTextLength: 36)]
        public Guid RunId { get; set; }
    }
}

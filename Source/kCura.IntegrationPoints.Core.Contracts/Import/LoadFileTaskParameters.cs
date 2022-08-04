using System;

namespace kCura.IntegrationPoints.Core.Contracts.Import
{
    [Serializable]
    public class LoadFileTaskParameters
    {
        public int ProcessedItemsCount { get; set; }

        public long Size { get; set; }

        public DateTime LastModifiedDate { get; set; }
    }
}

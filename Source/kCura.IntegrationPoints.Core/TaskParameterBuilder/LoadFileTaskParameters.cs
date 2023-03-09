using System;

namespace kCura.IntegrationPoints.Core
{
    [Serializable]
    public class LoadFileTaskParameters
    {
        public int ProcessedItemsCount { get; set; }

        public long Size { get; set; }

        public DateTime LastModifiedDate { get; set; }
    }
}

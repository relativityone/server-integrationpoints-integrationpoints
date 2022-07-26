using Relativity.Sync.Configuration;

namespace Relativity.Sync.Tests.Performance.Helpers
{
    public class PerformanceTestCase
    {
        public string TestCaseName { get; set; }
        public bool MapExtractedText { get; set; } = true;
        public int NumberOfMappedFields { get; set; }
        public ImportNativeFileCopyMode CopyMode { get; set; } = ImportNativeFileCopyMode.CopyFiles;
        public ImportOverwriteMode OverwriteMode { get; set; } = ImportOverwriteMode.AppendOnly;
        public int ExpectedItemsTransferred { get; set; }
    }
}

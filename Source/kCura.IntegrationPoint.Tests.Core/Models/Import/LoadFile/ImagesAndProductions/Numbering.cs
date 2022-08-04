using System.ComponentModel;

namespace kCura.IntegrationPoint.Tests.Core.Models.Import.LoadFile.ImagesAndProductions
{
    public enum Numbering
    {
        [Description("Use load file page IDs")]
        UseLoadFilePageIds,

        [Description("Auto-number pages")]
        AutoNumberPages
    }
}

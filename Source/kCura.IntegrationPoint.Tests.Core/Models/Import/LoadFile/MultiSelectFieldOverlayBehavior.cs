using System.ComponentModel;

namespace kCura.IntegrationPoint.Tests.Core.Models.Import.LoadFile
{
    public enum MultiSelectFieldOverlayBehavior
    {
        [Description("Merge Values")]
        MergeValues,

        [Description("Replace Values")]
        ReplaceValues,

        [Description("Use Field Settings")]
        UseFieldSettings
    }
}

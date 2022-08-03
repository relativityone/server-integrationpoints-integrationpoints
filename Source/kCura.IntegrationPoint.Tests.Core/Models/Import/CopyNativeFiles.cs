using System.ComponentModel;

namespace kCura.IntegrationPoint.Tests.Core.Models.Import
{
    public enum CopyNativeFiles
    {
        [Description("Physical files")]
        PhysicalFiles,

        [Description("Links Only")]
        LinksOnly,

        [Description("No")]
        No
    }
}

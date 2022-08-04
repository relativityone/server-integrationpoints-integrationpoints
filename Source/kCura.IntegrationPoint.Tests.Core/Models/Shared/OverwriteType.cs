using System.ComponentModel;

namespace kCura.IntegrationPoint.Tests.Core.Models.Shared
{
    public enum OverwriteType
    {
        [Description("Append Only")]
        AppendOnly,

        [Description("Overlay Only")]
        OverlayOnly,

        [Description("Append/Overlay")]
        AppendOverlay
    }
}
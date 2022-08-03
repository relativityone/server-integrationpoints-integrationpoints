using System.ComponentModel;

namespace kCura.IntegrationPoint.Tests.Core.Models.Import.LoadFile
{
    public enum ImportType
    {
        [Description("Document Load File")]
        DocumentLoadFile,

        [Description("Image Load File")]
        ImageLoadFile,

        [Description("Production Load File")]
        ProductionLoadFile
    }
}

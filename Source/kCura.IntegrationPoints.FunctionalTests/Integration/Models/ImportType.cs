using System.ComponentModel;

namespace Relativity.IntegrationPoints.Tests.Integration.Models
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

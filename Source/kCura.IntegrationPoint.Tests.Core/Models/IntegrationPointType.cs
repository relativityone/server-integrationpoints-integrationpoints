using System.ComponentModel;

namespace kCura.IntegrationPoint.Tests.Core.Models
{
    public enum IntegrationPointType
    {
        [Description("Import")]
        Import,

        [Description("Export")]
        Export
    }
}

using Atata;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Models
{
    public enum IntegrationPointImportTypes
    {
        [Term("Select...")]
        Select,
        [Term("Document Load File")]
        DocumentLoadFile,
        [Term("Image Load File")]
        ImageLoadFile,
        [Term("Production Load File")]
        ProductionLoadFile,
    }
}

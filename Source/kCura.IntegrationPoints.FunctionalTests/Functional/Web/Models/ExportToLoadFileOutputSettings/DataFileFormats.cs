
using Atata;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Models.ExportToLoadFileOutputSettings
{
    public enum DataFileFormats
    {
        [Term("Select...")]
        Select,
        [Term("Relativity (.dat)")]
        Relativity,
        [Term("HTML (.html)")]
        HTML,
        [Term("Comma-separated (.csv)")]
        CSV,
        [Term("Custom (.txt)")]
        Custom
    }
}

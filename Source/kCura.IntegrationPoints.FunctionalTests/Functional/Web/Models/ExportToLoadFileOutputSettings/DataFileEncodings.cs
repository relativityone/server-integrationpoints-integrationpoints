using Atata;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Models.ExportToLoadFileOutputSettings
{
    public enum DataFileEncodings
    {
        Unicode,
        [Term("Unicode (Big-Endian)")]
        UnicodeBigEndian,
        [Term("Unicode (UTF-8)")]
        UnicodeUTF8,
        [Term("Western European (Windows)")]
        WesternEuropean
    }
}

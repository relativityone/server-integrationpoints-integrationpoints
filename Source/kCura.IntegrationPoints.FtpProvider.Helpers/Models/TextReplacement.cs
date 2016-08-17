using System;

namespace kCura.IntegrationPoints.FtpProvider.Helpers.Models
{
    public class TextReplacement
    {
        public Int32 StartIndex { get; set; }
        public Int32 EndIndex { get; set; }
        public String OriginalText { get; set; }
        public String UpdatedText { get; set; }
    }
}

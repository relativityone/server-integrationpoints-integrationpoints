using System;

namespace kCura.IntegrationPoints.FtpProvider.Helpers.Models
{
    public class TextReplacement
    {
        public int StartIndex { get; set; }

        public int EndIndex { get; set; }

        public string OriginalText { get; set; }

        public string UpdatedText { get; set; }
    }
}

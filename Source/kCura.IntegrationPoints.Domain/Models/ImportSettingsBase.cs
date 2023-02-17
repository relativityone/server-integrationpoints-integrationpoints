using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Domain.Models
{
    public class ImportSettingsBase
    {
        public int WorkspaceId { get; set; }

        public string ImportType { get; set; }

        public string HasStartLine { get; set; }

        public string LineNumber { get; set; }

        public string LoadFile { get; set; }

        public string EncodingType { get; set; }

        public int AsciiColumn { get; set; }

        public int AsciiQuote { get; set; }

        public int AsciiNewLine { get; set; }

        public int AsciiMultiLine { get; set; }

        public int AsciiNestedValue { get; set; }
    }
}

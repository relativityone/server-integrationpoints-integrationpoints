using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Domain.Models
{
    public class ImportPreviewSettings : ImportSettingsBase
    {
        public int PreviewType { get; set; }

        public List<FieldMap> FieldMapping { get; set; }

        public List<string> ChoiceFields { get; set; }

        public string ExtractedTextColumn { get; set; }

        public string ExtractedTextFileEncoding { get; set; }
    }
}

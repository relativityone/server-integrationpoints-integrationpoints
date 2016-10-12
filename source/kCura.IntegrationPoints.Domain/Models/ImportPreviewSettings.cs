using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Domain.Models
{
    public class ImportPreviewSettings : ImportSettingsBase
    {
        public string PreviewType { get; set; }
        public List<FieldMap> FieldMapping { get; set; }
    }
}

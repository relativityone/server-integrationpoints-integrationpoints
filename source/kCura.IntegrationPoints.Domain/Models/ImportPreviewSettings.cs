using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Domain.Models
{
    public class ImportPreviewSettings
    {
        public int WorkspaceId { get; set; }
        public string PreviewType { get; set; }
        public string FilePath { get; set; }
        public List<FieldMap> FieldMapping { get; set; }
    }
}

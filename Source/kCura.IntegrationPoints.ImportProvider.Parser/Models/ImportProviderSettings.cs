using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Models
{
    public class ImportProviderSettings
    {
        public string InputType { get; set; }
        public string HasStartLine { get; set; }
        public string LineNumber { get; set; }
        public string LoadFile { get; set; }
    }
}

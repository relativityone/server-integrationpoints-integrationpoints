using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Domain.Models
{
    public class ImportPreviewTable
    {
        public ImportPreviewTable()
        {
            Header = new List<string>();
            Data = new List<List<string>>();
            ErrorRows = new List<int>();
        }

        public List<string> Header { get; set; }
        public List<List<string>> Data { get; set; }
        public List<int> ErrorRows { get; set; }
    }
}

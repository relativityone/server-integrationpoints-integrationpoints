using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Domain.Models
{
    public class ImportPreviewStatus
    {
        public bool IsComplete { get; set; }

        public bool IsFailed { get; set; }

        public string ErrorMessage { get; set; }

        public long TotalBytes { get; set; }

        public long BytesRead { get; set; }

        public long StepSize { get; set; }
    }
}

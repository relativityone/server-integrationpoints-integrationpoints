using System.Collections.Generic;

namespace kCura.IntegrationPoints.Core.Models
{
    public class EmailJobParameters
    {
        public string Subject { get; set; }
        public string MessageBody { get; set; }
        public IEnumerable<string> Emails { get; set; } 
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Core.Models
{
	public class EmailMessage
	{
		public string Subject { get; set; }
		public string MessageBody { get; set; }
		public IEnumerable<string> Emails { get; set; } 
	}
}

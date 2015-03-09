using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace kCura.IntegrationPoints.Email
{
	public class EmailConfiguration
	{
		public string Domain { get; set; }
		public int Port { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
		public bool UseSSL{ get; set; }
	}
}

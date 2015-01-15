using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.SourceProviderInstaller
{
	public class SourceProvider
	{
		internal Guid GUID { get; set; }
		public string Name { get; set; }
		public string FileLocation { get; set; }
	}
}

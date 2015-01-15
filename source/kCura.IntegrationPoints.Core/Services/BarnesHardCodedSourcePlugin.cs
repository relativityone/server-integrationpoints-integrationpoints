using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Core.Services
{
	public class BarnesHardCodedSourcePlugin : ISourcePluginProvider
	{
		public FileStream[] GetPluginLibraries(Guid selector)
		{
			return new FileStream[] { File.OpenRead(@"C:\SourceCode\LDAPSync\example\JsonLoader\JsonLoader\bin\JsonLoader_merge.dll") };
		}
	}
}

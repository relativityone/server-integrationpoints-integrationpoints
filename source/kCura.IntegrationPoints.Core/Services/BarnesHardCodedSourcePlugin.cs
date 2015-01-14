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
		public Stream[] GetPluginLibraries(Guid selector)
		{
			throw new NotImplementedException();
		}
	}
}

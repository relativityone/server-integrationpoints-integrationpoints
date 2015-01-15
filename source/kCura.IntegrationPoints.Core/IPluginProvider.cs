using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Core
{
	public interface IPluginProvider
	{
		FileStream[] GetPluginLibraries(Guid selector);
	}
}

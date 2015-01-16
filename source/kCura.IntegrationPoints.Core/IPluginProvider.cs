using System;
using System.Collections.Generic;
using System.IO;

namespace kCura.IntegrationPoints.Core
{
	public interface IPluginProvider
	{
		Guid ApplicationGuid { get; set; }
		IEnumerable<Stream> GetPluginLibraries(Guid selector);
	}
}

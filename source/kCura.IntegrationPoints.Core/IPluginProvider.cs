using System;
using System.Collections.Generic;
using System.IO;

namespace kCura.IntegrationPoints.Core
{
	public interface IPluginProvider
	{
		IEnumerable<Stream> GetPluginLibraries(Guid applicationGuid);
	}
}

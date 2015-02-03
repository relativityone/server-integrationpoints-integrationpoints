using System;
using System.Collections.Generic;
using System.IO;
using kCura.IntegrationPoints.Data.Models;

namespace kCura.IntegrationPoints.Core
{
	public interface IPluginProvider
	{
		IDictionary<ApplicationBinary, Stream> GetPluginLibraries(Guid applicationGuid);
	}
}

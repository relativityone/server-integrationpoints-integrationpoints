using System;
using System.IO;

namespace kCura.IntegrationPoints.Core
{
	public interface IPluginProvider
	{
		Guid ApplicationGuid { get; set; }
		FileStream[] GetPluginLibraries(Guid selector);
	}
}

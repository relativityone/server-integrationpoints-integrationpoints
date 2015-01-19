using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using kCura.IntegrationPoints.Data.Queries;

namespace kCura.IntegrationPoints.Core.Services.Provider
{
	public class DefaultSourcePluginProvider : ISourcePluginProvider
	{
		private readonly GetApplicationBinaries _getApplicationBinaries;
		public DefaultSourcePluginProvider(GetApplicationBinaries getApplicationBinaries)
		{
			_getApplicationBinaries = getApplicationBinaries;
		}

		public IEnumerable<Stream> GetPluginLibraries(Guid applicationGuid)
		{
			var apps = _getApplicationBinaries.Execute(applicationGuid);
			return apps.Select(applicationBinary => new MemoryStream(applicationBinary.FileData));
		}

		static public string AssemblyLoadDirectory
		{
			get
			{
				string codeBase = Assembly.GetCallingAssembly().CodeBase;
				UriBuilder uri = new UriBuilder(codeBase);
				string path = Uri.UnescapeDataString(uri.Path);
				return Path.GetDirectoryName(path);
			}
		}
	}
}

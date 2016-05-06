using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using kCura.IntegrationPoints.Data.Models;
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

		public IDictionary<ApplicationBinary, Stream> GetPluginLibraries(Guid applicationGuid)
		{
			List<ApplicationBinary> apps = _getApplicationBinaries.Execute(applicationGuid);
			return apps.ToDictionary(appBinary => appBinary, appBinary => (Stream)new MemoryStream(appBinary.FileData));
		}

		public static string AssemblyLoadDirectory
		{
			get
			{
				string codeBase = System.Reflection.Assembly.GetCallingAssembly().CodeBase;
				UriBuilder uri = new UriBuilder(codeBase);
				string path = Uri.UnescapeDataString(uri.Path);
				return Path.GetDirectoryName(path);
			}
		}
	}
}

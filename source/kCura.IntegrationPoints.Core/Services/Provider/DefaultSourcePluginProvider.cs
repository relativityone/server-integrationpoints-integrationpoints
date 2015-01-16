using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using kCura.IntegrationPoints.Core.Queries;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.Queries;

namespace kCura.IntegrationPoints.Core.Services.Provider
{
	public class DefaultSourcePluginProvider : ISourcePluginProvider
	{
		private readonly GetSourceProviderRdoByIdentifier _sourceProviderIdentifier;
		private readonly GetApplicationBinaries _getApplicationBinaries;
		public DefaultSourcePluginProvider(GetSourceProviderRdoByIdentifier sourceProviderIdentifier, GetApplicationBinaries getApplicationBinaries)
		{
			_sourceProviderIdentifier = sourceProviderIdentifier;
			_getApplicationBinaries = getApplicationBinaries;
		}

		private Guid _applicationGuid = Guid.Empty;
		public Guid ApplicationGuid
		{
			get
			{
				if (_applicationGuid == Guid.Empty)
				{
					_applicationGuid = Guid.Parse(_sourceProviderIdentifier.Execute(_providerGuid).ApplicationIdentifier);
				}
				return _applicationGuid;
			}
			set { _applicationGuid = value; }
		}

		private Guid _providerGuid = Guid.Empty;
		
		public IEnumerable<Stream> GetPluginLibraries(Guid selector)
		{
			_providerGuid = selector;
			var apps = _getApplicationBinaries.Execute(ApplicationGuid);
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

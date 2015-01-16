using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using kCura.IntegrationPoints.Core.Queries;
using kCura.IntegrationPoints.Core.Services.ServiceContext;

namespace kCura.IntegrationPoints.Core.Services.Provider
{
	public class DefaultSourcePluginProvider : ISourcePluginProvider
	{
		private ICaseServiceContext _caseContext;
		private IEddsServiceContext _eddsContext;
		public DefaultSourcePluginProvider(ICaseServiceContext caseContext, IEddsServiceContext eddsContext)
		{
			_caseContext = caseContext;
			_eddsContext = eddsContext;
		}

		private Guid _applicationGuid = Guid.Empty;
		public Guid ApplicationGuid
		{
			get
			{
				if (_applicationGuid == Guid.Empty)
				{
					_applicationGuid = Guid.Parse(new GetSourceProviderRdoByIdentifier(_caseContext).Execute(_providerGuid).ApplicationIdentifier);
				}
				return _applicationGuid;
			}
			set { _applicationGuid = value; }
		}

		private Guid _providerGuid = Guid.Empty;
		public FileStream[] GetPluginLibraries(Guid selector)
		{
			_providerGuid = selector;

			if (selector.Equals(Guid.Parse("4380b80b-57ef-48c3-bf02-b98d2855166b")))
			{
				return new FileStream[]
				{
					File.OpenRead(@"C:\SourceCode\LDAPSync\example\JsonLoader\JsonLoader\bin\JsonLoader_merge.dll")
				};
			}
			else
			{
				return new FileStream[]
				{
					File.OpenRead(@"C:\SourceCode\LDAPSync\source\kCura.IntegrationPoints.LDAPProvider\bin\Newtonsoft.Json.dll"),
					File.OpenRead(@"C:\SourceCode\LDAPSync\source\kCura.IntegrationPoints.LDAPProvider\bin\kCura.IntegrationPoints.LDAPProvider.dll")
					//File.OpenRead(@"C:\SourceCode\LDAPSync\source\kCura.IntegrationPoints.LDAPProvider\bin\kCura.IntegrationPoints.LDAPProvider_merge.dll")
				};
			}
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

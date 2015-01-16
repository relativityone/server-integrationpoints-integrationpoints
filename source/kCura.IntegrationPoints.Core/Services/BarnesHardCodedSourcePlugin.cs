using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace kCura.IntegrationPoints.Core.Services
{
	public class BarnesHardCodedSourcePlugin : ISourcePluginProvider
	{
		public IEnumerable<Stream> GetPluginLibraries(Guid selector)
		{
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

		public Guid ApplicationGuid
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace kCura.IntegrationPoints.Core.Services
{
	public class BarnesHardCodedSourcePlugin : ISourcePluginProvider
	{
		public FileStream[] GetPluginLibraries(Guid selector)
		{
			List<FileStream> streams = new List<FileStream>();
			if (selector == Guid.Parse("5bf1f2c2-9670-4d6e-a3e9-dbc83db6c232"))
			{
				//LDAP
				//string dllPath = Path.Combine(@"C:\SourceCode\LDAPSync\source\bin\Debug", "kCura.LDAPProvider.dll");
				//streams.Add(File.OpenRead(dllPath));
				//string dllPath = Path.Combine(AssemblyLoadDirectory, "Newtonsoft.Json.dll");
				//streams.Add(File.OpenRead(dllPath));
				//dllPath = Path.Combine(AssemblyLoadDirectory, "kCura.Apps.Common.Utils.dll");
				//streams.Add(File.OpenRead(dllPath));
				//dllPath = Path.Combine(AssemblyLoadDirectory, "kCura.IntegrationPoints.LDAPProvider.dll");
				//streams.Add(File.OpenRead(dllPath));
			}
			else
			{
				//JSON sample
				string dllPath = @"C:\SourceCode\LDAPSync\example\JsonLoader\JsonLoader\bin\JsonLoader_merge.dll";
				streams.Add(File.OpenRead(dllPath));
			}
			return streams.ToArray();
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts;

namespace kCura.IntegrationPoints.Core
{
	public class DomainHelper
	{
		public virtual void LoadRequiredAssemblies(AppDomain domain)
		{
			//loads the contracts dll into the app domain so we can reference 
			var assemblyPath = new Uri(typeof(Contracts.AssemblyDomainLoader).Assembly.CodeBase).LocalPath;
			byte[] stream = File.ReadAllBytes(assemblyPath);
			var dir = Path.Combine(domain.BaseDirectory, new FileInfo(assemblyPath).Name);
			File.WriteAllBytes(dir, stream);
			domain.Load(stream);
		}

		public virtual T CreateInstance<T>(AppDomain domain) where T : class
		{
			var type = typeof(T);
			var instance = domain.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName) as T;
			if (instance == null)
			{
				throw new Exception(string.Format("Could not create an instance of {0} in app domain {1}.", type.Name, domain.FriendlyName));
			}
			return instance;
		}

		public virtual void LoadClientLibraries(AppDomain domain, IPluginProvider provider, Guid identifier)
		{
			var loader = this.CreateInstance<Contracts.AssemblyDomainLoader>(domain);
			FileStream[] assemblies = provider.GetPluginLibraries(identifier);
			foreach (var stream in assemblies)
			{
				stream.Seek(0, SeekOrigin.Begin);
				loader.Load(stream);
				stream.Dispose();
			}
		}

		public virtual void ReleaseDomain(AppDomain domain)
		{
			if (domain != null)
			{
				Directory.Delete(domain.BaseDirectory, true);
				AppDomain.Unload(domain);
			}
		}

		public virtual AppDomain CreateNewDomain()
		{
			AppDomainSetup domaininfo = new AppDomainSetup();
			var domainPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			Directory.CreateDirectory(domainPath);
			domaininfo.ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
			domaininfo.ApplicationBase = domainPath;
			string domainName = Guid.NewGuid().ToString();
			var newDomain = AppDomain.CreateDomain(domainName, null, domaininfo);
			return newDomain;
		}

		public virtual DomainManager SetupDomainAndCreateManager(AppDomain domain, IPluginProvider provider, Guid identifer)
		{
			this.LoadRequiredAssemblies(domain);
			this.LoadClientLibraries(domain, provider, identifer);
			var manager = this.CreateInstance<Contracts.DomainManager>(domain);
			manager.Init();
			return manager;
		}

	}
}

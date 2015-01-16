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
			var assemblyPath = new Uri(typeof(Contracts.AssemblyDomainLoader).Assembly.CodeBase).LocalPath; //TODO switch to this instead
			//var assemblyPath = @"C:\SourceCode\LDAPSync\source\bin\Debug\kCura.IntegrationPoints.Contracts.dll";
			var stream = File.ReadAllBytes(assemblyPath);
			var dir = Path.Combine(domain.BaseDirectory, new FileInfo(assemblyPath).Name);
			File.WriteAllBytes(dir, stream);
			domain.Load(stream);
		}

		public virtual T CreateInstance<T>(AppDomain domain) where T : class
		{
			var type = typeof(T);
			var instance = domain.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName) as T;
			//dynamic instance = domain.CreateInstanceAndUnwrap("kCura.IntegrationPoints.Contracts", type.FullName);
			if (instance == null)
			{
				throw new Exception(string.Format("Could not create an instance of {0} in app domain {1}.", type.Name, domain.FriendlyName));
			}
			return instance;
		}

		private byte[] ReadFully(Stream stream)
		{
			byte[] buffer = new byte[16 * 1024];
			using (MemoryStream ms = new MemoryStream())
			{
				int read;
				while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
				{
					ms.Write(buffer, 0, read);
				}
				stream.Seek(0, SeekOrigin.Begin);
				return ms.ToArray();
			}
		}

		public virtual void LoadClientLibraries(AppDomain domain, IPluginProvider provider, Guid identifier)
		{
			var loader = this.CreateInstance<Contracts.AssemblyDomainLoader>(domain);
			var assemblies = provider.GetPluginLibraries(identifier);
			foreach (var stream in assemblies)
			{
				stream.Seek(0, SeekOrigin.Begin);
				var file = Path.Combine(domain.BaseDirectory, Guid.NewGuid().ToString() + ".dll");
				File.WriteAllBytes(file, ReadFully(stream));
				loader.LoadFrom(file);
				stream.Dispose();
			}
		}

		public virtual void ReleaseDomain(AppDomain domain)
		{
			if (domain != null)
			{
				domain.DomainUnload += (sender, args) =>
				{


				};
				AppDomain.Unload(domain);

				Directory.Delete(domain.BaseDirectory, true);
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

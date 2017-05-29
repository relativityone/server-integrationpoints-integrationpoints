using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using kCura.IntegrationPoints.Contracts.Domain;
using Relativity;
using Relativity.APIHelper;

namespace kCura.IntegrationPoints.Domain
{
	public class DomainHelper
	{
		private AssemblyDomainLoader _appDomainLoader;

		public virtual void LoadRequiredAssemblies(AppDomain domain)
		{
			//loads the contracts dll into the app domain so we have reference 
			var assemblyLocalPath = new Uri(typeof(AssemblyDomainLoader).Assembly.CodeBase).LocalPath;
			var assemblyLocalDirectory = Path.GetDirectoryName(assemblyLocalPath);
			var assemblyPath = Path.Combine(assemblyLocalDirectory, "kCura.IntegrationPoints.Contracts.dll");
			if (!File.Exists(assemblyPath))
			{
				throw new FileNotFoundException(
					$"Required file kCura.IntegrationPoints.Contracts.dll is missing in {assemblyLocalDirectory}.");
			}
			var stream = File.ReadAllBytes(assemblyPath);
			var dir = Path.Combine(domain.BaseDirectory, new FileInfo(assemblyPath).Name);
			File.WriteAllBytes(dir, stream);
			assemblyPath = Path.Combine(assemblyLocalDirectory, "kCura.IntegrationPoints.Domain.dll");
			if (!File.Exists(assemblyPath))
			{
				throw new FileNotFoundException(
					$"Required file kCura.IntegrationPoints.Domain.dll is missing in {assemblyLocalDirectory}.");
			}
			stream = File.ReadAllBytes(assemblyPath);
			dir = Path.Combine(domain.BaseDirectory, new FileInfo(assemblyPath).Name);
			File.WriteAllBytes(dir, stream);

			domain.Load(AssemblyName.GetAssemblyName(dir).Name);
			_appDomainLoader = this.CreateInstance<AssemblyDomainLoader>(domain);
			domain.AssemblyResolve += new ResolveEventHandler(_appDomainLoader.ResolveAssembly);
		}

		public virtual T CreateInstance<T>(AppDomain domain) where T : class
		{
			var type = typeof(T);
			var instance = domain.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName) as T;
			if (instance == null)
			{
				throw new Exception(string.Format("Could not create an instance of {0} in app domain {1}.", type.Name,
					domain.FriendlyName));
			}
			return instance;
		}

		private byte[] ReadFully(Stream stream)
		{
			byte[] buffer = new byte[16*1024];
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

		public virtual void LoadClientLibraries(AppDomain domain, IPluginProvider provider, Guid applicationGuid)
		{
			List<System.Reflection.Assembly> domainAssemblies = domain.GetAssemblies().ToList();

			IDictionary<ApplicationBinary, Stream> assemblies = provider.GetPluginLibraries(applicationGuid);
			List<string> files = new List<string>();
			foreach (ApplicationBinary appBinary in assemblies.Keys)
			{
				Stream stream = assemblies[appBinary];
				stream.Seek(0, SeekOrigin.Begin);
				var file = Path.Combine(domain.BaseDirectory, appBinary.Name);
				if (!File.Exists(file)) File.WriteAllBytes(file, ReadFully(stream));
				files.Add(file);
				stream.Dispose();
			}
			IDictionary<string, System.Reflection.Assembly> loadedAssemblies =
				new Dictionary<string, System.Reflection.Assembly>();
			foreach (System.Reflection.Assembly assembly in domainAssemblies)
			{
				string name = assembly.GetName().Name;
				if (!loadedAssemblies.ContainsKey(name))
					loadedAssemblies.Add(name, assembly);
			}
			foreach (string file in files)
			{
				AssemblyName assemblyName = AssemblyName.GetAssemblyName(file);
				if (!loadedAssemblies.ContainsKey(assemblyName.Name))
				{
					_appDomainLoader.LoadFrom(file);
				}
			}
		}

		public virtual void ReleaseDomain(AppDomain domain)
		{
			if (domain != null)
			{
				string domainDirectory = domain.BaseDirectory;
				if (_appDomainLoader != null)
				{
					try
					{
						domain.AssemblyResolve -= _appDomainLoader.ResolveAssembly;
					}
					catch
					{
					}
					_appDomainLoader = null;
				}
				try
				{
					domain.DomainUnload += (sender, args) => { };
				}
				catch
				{
				}
				AppDomain.Unload(domain);

				try
				{
					Directory.Delete(domainDirectory, true);
				}
				catch
				{
				}
			}
		}

		public virtual AppDomain CreateNewDomain(RelativityFeaturePathService relativityFeaturePathService)
		{
			AppDomainSetup domaininfo = new AppDomainSetup();
			var domainPath = Path.Combine(Path.GetTempPath(), "RelativityIntegrationPoints");
			domainPath = Path.Combine(domainPath, Guid.NewGuid().ToString());
			Directory.CreateDirectory(domainPath);
			domaininfo.ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
			domaininfo.ApplicationBase = domainPath;
			domaininfo.PrivateBinPath =
				Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath);
			string domainName = Guid.NewGuid().ToString();
			var newDomain = AppDomain.CreateDomain(domainName, null, domaininfo);
			DeployLibraryFiles(domainPath, relativityFeaturePathService);
			return newDomain;
		}

		public virtual DomainManager SetupDomainAndCreateManager(AppDomain domain, IPluginProvider provider,
			Guid applicationGuid)
		{
			this.LoadRequiredAssemblies(domain);
			this.LoadClientLibraries(domain, provider, applicationGuid);
			DomainManager manager = this.CreateInstance<DomainManager>(domain);

			IDictionary<string, byte[]> dataDictionary = new Dictionary<string, byte[]>();

			// Get the connection string
			string connectionString = kCura.Config.Config.ConnectionString;
			byte[] connectionStringBytes = Encoding.ASCII.GetBytes(connectionString);
			dataDictionary.Add(Constants.IntegrationPoints.APP_DOMAIN_DATA_CONNECTION_STRING, connectionStringBytes);

			// Marshal the data
			IAppDomainDataMarshaller dataMarshaller = new SecureAppDomainDataMarshaller();
			dataMarshaller.MarshalDataToDomain(domain, dataDictionary);

			manager.Init();

			Bootstrapper.InitAppDomain(Constants.IntegrationPoints.APP_DOMAIN_SUBSYSTEM_NAME,
				Constants.IntegrationPoints.APPLICATION_GUID_STRING, domain);

			return manager;
		}

		private void DeployLibraryFiles(string finalDllPath, RelativityFeaturePathService relativityFeaturePathService)
		{
			string libDllPath = null;

			libDllPath = relativityFeaturePathService.LibraryPath;
			CopyDirectoryFiles(libDllPath, finalDllPath, true, true);

			//kCura.Agent
			libDllPath = relativityFeaturePathService.WebProcessingPath;
			if (!string.IsNullOrWhiteSpace(libDllPath)) CopyFileWithWildcard(libDllPath, finalDllPath, "kCura.Agent*");

			//FSharp.Core
			libDllPath = relativityFeaturePathService.EddsPath;
			if (!string.IsNullOrWhiteSpace(libDllPath))
				CopyFileWithWildcard(Path.Combine(libDllPath, "bin"), finalDllPath, "FSharp.Core*");
		}

		private void CopyFileWithWildcard(string sourceDir, string targetDir, string fileName)
		{
			string[] files = Directory.GetFiles(sourceDir, fileName);
			foreach (string file in files)
			{
				FileInfo file_info = new FileInfo(file);
				File.Copy(file, Path.Combine(targetDir, file_info.Name), true);
			}
		}

		private void CopyDirectoryFiles(string sourceDir, string targetDir, bool overwriteFiles, bool includeSubdirectories)
		{
			Directory.CreateDirectory(targetDir);

			foreach (var file in Directory.GetFiles(sourceDir))
			{
				File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)), overwriteFiles);
			}
			if (includeSubdirectories)
			{
				foreach (var directory in Directory.GetDirectories(sourceDir))
				{
					CopyDirectoryFiles(directory, Path.Combine(targetDir, Path.GetFileName(directory)), overwriteFiles,
						includeSubdirectories);
				}
			}
		}

		private void PrepAssemblies(AppDomain domain)
		{
			foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				string assemblyName = assembly.GetName().Name;
				if (!IsSystemDependencyName(assemblyName))
				{
					File.Copy(assembly.Location, Path.Combine(domain.BaseDirectory, Path.GetFileName(assembly.Location)), true);
				}
			}
		}

		private bool IsSystemDependencyName(string assemblyName)
		{
			string potentialSystemAssemblyName = assemblyName.ToLower();
			if (potentialSystemAssemblyName.Equals("system"))
			{
				return true;
			}
			if (potentialSystemAssemblyName.Equals("mscorlib"))
			{
				return true;
			}
			if (potentialSystemAssemblyName.StartsWith("system."))
			{
				return true;
			}
			if (potentialSystemAssemblyName.StartsWith("microsoft."))
			{
				return true;
			}
			return false;
		}
	}
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Castle.Core.Internal;
using kCura.IntegrationPoints.Domain.Toggles;
using Polly;
using Relativity.API;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Domain
{
	public class AppDomainHelper : IAppDomainHelper
	{
		private const ushort _MAX_NUMBER_OF_DIRECTORY_DELETE_RETRIES = 3;
		private const ushort _DIRECTORY_DELETE_RETRIES_WAIT_TIME_BASE_IN_SECONDS = 3;

		private AssemblyDomainLoader _appDomainLoader;
		private readonly IPluginProvider _pluginProvider;
		private readonly IHelper _helper;
		private readonly IAPILog _logger;
		private readonly RelativityFeaturePathService _relativityFeaturePathService;
		private readonly IToggleProvider _toggleProvider;

        public AppDomainHelper(IPluginProvider pluginProvider, IHelper helper, RelativityFeaturePathService relativityFeaturePathService, IToggleProvider toggleProvider)
		{

			_pluginProvider = pluginProvider;
			_helper = helper;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<AppDomainHelper>();
			_relativityFeaturePathService = relativityFeaturePathService;
            _toggleProvider = toggleProvider;
        }

		private T CreateInstance<T>(AppDomain domain, params object[] constructorArgs) where T : class
		{
			Type type = typeof(T);
			var instance = domain.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName, false, BindingFlags.Default, null, constructorArgs, null, null) as T;
			if (instance == null)
			{
				throw new Exception(string.Format("Could not create an instance of {0} in app domain {1}.", type.Name,
					domain.FriendlyName));
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

		public virtual void LoadClientLibraries(AppDomain domain, Guid applicationGuid)
		{
			if (_appDomainLoader == null)
			{
				_appDomainLoader = CreateInstance<AssemblyDomainLoader>(domain);
			}
			List<System.Reflection.Assembly> domainAssemblies = domain.GetAssemblies().ToList();
			IDictionary<ApplicationBinary, Stream> assemblies = _pluginProvider.GetPluginLibraries(applicationGuid);
			List<string> files = new List<string>();
			foreach (ApplicationBinary appBinary in assemblies.Keys)
			{
				Stream stream = assemblies[appBinary];
				stream.Seek(0, SeekOrigin.Begin);
				var file = Path.Combine(domain.BaseDirectory, appBinary.Name);
				if (!File.Exists(file))
				{
					File.WriteAllBytes(file, ReadFully(stream));
				}
				files.Add(file);
				stream.Dispose();
			}
			IDictionary<string, System.Reflection.Assembly> loadedAssemblies =
				new Dictionary<string, System.Reflection.Assembly>();
			foreach (System.Reflection.Assembly assembly in domainAssemblies)
			{
				string name = assembly.GetName().Name;
				if (!loadedAssemblies.ContainsKey(name))
				{
					loadedAssemblies.Add(name, assembly);
				}
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
				string domainName = domain.FriendlyName;
				string domainPath = domain.BaseDirectory;

				if (_appDomainLoader != null)
				{
					try
					{
						domain.AssemblyResolve -= _appDomainLoader.ResolveAssembly;
					}
					catch
					{ }

					_appDomainLoader = null;
				}

				try
				{
					domain.DomainUnload += (sender, args) => { };
				}
				catch
				{ }

				AppDomain.Unload(domain);

				try
				{
					_logger.LogInformation("Removing directory {domainPath} for domain {domainName}.", domainPath, domainName);

					Policy
						.Handle<Exception>()
						.WaitAndRetry(
							_MAX_NUMBER_OF_DIRECTORY_DELETE_RETRIES,
							(retryAttempt) => TimeSpan.FromSeconds(Math.Pow(_DIRECTORY_DELETE_RETRIES_WAIT_TIME_BASE_IN_SECONDS, retryAttempt)))
						.Execute(() => Directory.Delete(domainPath, true));
				}
				catch (Exception ex)
				{
					_logger.LogWarning(ex, "Removing directory {domainPath} for domain {domainName} has failed.", domainPath, domainName);
				}
			}
		}

		public virtual AppDomain CreateNewDomain()
		{
			string domainPath = Path.Combine(Path.GetTempPath(), "RelativityIntegrationPoints", Guid.NewGuid().ToString());
			Directory.CreateDirectory(domainPath);

			AppDomainSetup domainInfo = new AppDomainSetup
			{
				ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile,
				ApplicationBase = domainPath,
				PrivateBinPath = Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath)
			};

			string domainName = Guid.NewGuid().ToString();
			AppDomain newDomain = AppDomain.CreateDomain(domainName, null, domainInfo);

			_logger.LogInformation("Deploying library files for domain {domainName} to path {domainPath}.", domainName, domainPath);


            if (_toggleProvider.IsEnabled<LoadRequiredAssembliesInKubernetesMode>())
            {
                _logger.LogInformation("Required Assemblies Loading in Kubernetes Mode");
				DeployLibraryFiles(domainPath);
                LoadRequiredAssemblies(newDomain);
			}
            else
            {
                CopyLibraryFilesFromWorkingDirectory(domainPath);
			}

            return newDomain;
		}

        public virtual IAppDomainManager SetupDomainAndCreateManager(AppDomain domain,
			Guid applicationGuid)
		{
            LoadClientLibraries(domain, applicationGuid);
			AppDomainManager manager = CreateInstance<AppDomainManager>(domain, _helper);

			manager.Init();

			Bootstrappers.AppDomainBootstrapper.Bootstrap(
				Constants.IntegrationPoints.APP_DOMAIN_SUBSYSTEM_NAME,
				Constants.IntegrationPoints.APPLICATION_GUID_STRING, 
				libraryPath: string.Empty, 
				domain: domain);

			return manager;
		}

        private void CopyLibraryFilesFromWorkingDirectory(string finalDllPath)
        {
            string assemblyLocalPath = new Uri(typeof(AssemblyDomainLoader).Assembly.CodeBase).LocalPath;
            string assemblyLocalDirectory = Path.GetDirectoryName(assemblyLocalPath);

            if (!assemblyLocalDirectory.IsNullOrEmpty())
            {
                CopyDirectoryFiles(assemblyLocalDirectory, finalDllPath, true, true);
            }
            else
            {
				_logger.LogError("assemblyLocalDirectory directory not found; " + assemblyLocalPath);
            }
		}

		private void DeployLibraryFiles(string finalDllPath)
		{
            string libDllPath = _relativityFeaturePathService.LibraryPath;
			CopyDirectoryFiles(libDllPath, finalDllPath, true, true);

            //kCura.Agent
			libDllPath = _relativityFeaturePathService.WebProcessingPath;
			if (!string.IsNullOrWhiteSpace(libDllPath))
			{
				CopyFileWithWildcard(libDllPath, finalDllPath, "kCura.Agent*");
			}

			//FSharp.Core
			libDllPath = _relativityFeaturePathService.EddsPath;
			if (!string.IsNullOrWhiteSpace(libDllPath))
			{
				CopyFileWithWildcard(Path.Combine(libDllPath, "bin"), finalDllPath, "FSharp.Core*");
			}
		}

        private void LoadRequiredAssemblies(AppDomain domain)
        {
            //loads the contracts dll into the app domain so we have reference 
            var assemblyLocalPath = new Uri(typeof(AssemblyDomainLoader).Assembly.CodeBase).LocalPath;
            var assemblyLocalDirectory = Path.GetDirectoryName(assemblyLocalPath);
            var assemblyPath = Path.Combine(assemblyLocalDirectory, "Relativity.IntegrationPoints.Contracts.dll");
            if (!File.Exists(assemblyPath))
            {
                throw new FileNotFoundException(
                    $"Required file Relativity.IntegrationPoints.Contracts.dll is missing in {assemblyLocalDirectory}.");
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
            _appDomainLoader = CreateInstance<AssemblyDomainLoader>(domain);
            domain.AssemblyResolve += _appDomainLoader.ResolveAssembly;
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
	}
}
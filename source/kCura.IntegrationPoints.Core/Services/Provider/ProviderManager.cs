using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Castle.Windsor.Installer;
using kCura.Crypto.DataProtection;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Domain;
using Relativity.API;
using Relativity.APIHelper;

namespace kCura.IntegrationPoints.Contracts
{
	/// <summary>
	/// Entry point into the App domain to create the Provider
	/// Internal use only:
	/// This class should not be referenced by any consuming library
	///  </summary>
	public class DomainManager : MarshalByRefObject
	{
		private IProviderFactory _providerFactory;
		private ISynchronizerFactory _synchronizerFactory;
		private WindsorContainer _windsorContainer;

		/// <summary>
		/// Called to initilized the provider's app domain and do any setup work needed
		/// </summary>
		public void Init()
		{
			// Resolve new app domain's assemlbies
			AppDomain.CurrentDomain.AssemblyResolve += AssemblyDomainLoader.ResolveAssembly;
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			Type startupType = typeof(IStartUp);
			var types = (from a in assemblies
									 from t in a.GetTypes()
									 where startupType.IsAssignableFrom(t) && t != startupType
									 select t).ToList();
			if (types.Any())
			{
				var type = types.FirstOrDefault();
				if (type != default(Type))
				{
					var instance = Activator.CreateInstance(type) as IStartUp;
					if (instance != null)
					{
						instance.Execute();
					}
				}
			}

			// Run bootstrapper for app domain
			Bootstrapper.InitAppDomain(Constants.IntegrationPoints.AppDomain_Subsystem_Name, Constants.IntegrationPoints.Application_GuidString, AppDomain.CurrentDomain);

			// Get marshaled data
			var dataProtector = new kCura.Crypto.DataProtection.DataProtector(Store.MachineStore);
			this.SetUpSystemToken(dataProtector);
			this.SetUpConnectionString(dataProtector);
		}

		/// <summary>
		/// Sets the SystemTokenProvider for the domain by retrieving the encrytped AppDomain data
		/// </summary>
		/// <param name="dataProtector">A DataProtector instance to use for decyrpting the AppDomain data</param>
		private void SetUpSystemToken(kCura.Crypto.DataProtection.DataProtector dataProtector)
		{
			byte[] data = this.RetrieveAppData(dataProtector, Constants.IntegrationPoints.AppDomain_Data_SystemTokenProvider);
			IProvideSystemTokens systemTokenProvider = this.ByteArrayToClass<IProvideSystemTokens>(data);
			if (systemTokenProvider != null)
			{
				ExtensionPointServiceFinder.SystemTokenProvider = systemTokenProvider;
			}
		}

		/// <summary>
		/// Sets the connection string for the domain by retrieving the encrypted AppDomain data
		/// </summary>
		/// <param name="dataProtector">A DataProtector instance to use for decyrpting the AppDomain data</param>
		private void SetUpConnectionString(kCura.Crypto.DataProtection.DataProtector dataProtector)
		{
			byte[] data = this.RetrieveAppData(dataProtector, Constants.IntegrationPoints.AppDomain_Data_ConnectionString);
			if (data != null)
			{
				string connectionString = System.Text.Encoding.ASCII.GetString(data);

				kCura.Config.Config.SetConnectionString(connectionString);
			}
		}

		private byte[] RetrieveAppData(kCura.Crypto.DataProtection.DataProtector dataProtector, string key)
		{
			byte[] encryptedData = AppDomain.CurrentDomain.GetData(key) as byte[];
			if (encryptedData == null)
			{
				return null;
			}

			byte[] decryptedData = dataProtector.Decrypt(encryptedData);

			return decryptedData;
		}

		// kudos to : http://stackoverflow.com/questions/4865104/convert-any-object-to-a-byte
		/// <summary>
		/// Convert a byte array to an Object
		/// </summary>
		/// <typeparam name="T">Type of object to create from byte array</typeparam>
		/// <param name="byteArray">Array of bytes representing class</param>
		/// <returns>An instance of the request type</returns>
		private T ByteArrayToClass<T>(byte[] byteArray) where T : class
		{
			if (byteArray == null)
			{
				return null;
			}

			var stream = new MemoryStream();
			var formatter = new BinaryFormatter();
			stream.Write(byteArray, 0, byteArray.Length);
			stream.Seek(0, SeekOrigin.Begin);
			T obj = formatter.Deserialize(stream) as T;

			return obj;
		}

		private void SetUpCastleWindsor()
		{
			_windsorContainer = new WindsorContainer();
			IKernel kernel = _windsorContainer.Kernel;
			kernel.Resolver.AddSubResolver(new CollectionResolver(kernel, true));
			var installerFactory = new InstallerFactory();
			_windsorContainer.Install(
				FromAssembly.InDirectory(
					new AssemblyFilter(AppDomain.CurrentDomain.RelativeSearchPath)
						.FilterByName(this.FilterByAllowedAssemblyNames)));
		}

		private bool FilterByAllowedAssemblyNames(AssemblyName assemblyName)
		{
			string[] allowedInstallerAssemblies = new[]
			{
				"kCura.IntegrationPoints", 
				"kCura.IntegrationPoints.Contracts", 
				"kCura.IntegrationPoints.Core", 
				"kCura.IntegrationPoints.Data"
			};

			if (allowedInstallerAssemblies.Contains(assemblyName.Name))
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Gets the provider in the app domain from the specific identifier
		/// </summary>
		/// <param name="identifer">The identifier that represents the provider</param>
		/// <param name="helper">Optional IHelper object to use for resolving classes</param>
		/// <returns>A Data source provider to retrieve data and pass along to the source.</returns>
		public Provider.IDataSourceProvider GetProvider(Guid identifer, IHelper helper)
		{
			if (_windsorContainer == null)
			{
				this.SetUpCastleWindsor();
			}

			if (_providerFactory == null)
			{
				if (helper != null)
				{
					if (!_windsorContainer.Kernel.HasComponent(typeof(IHelper)))
					{
						_windsorContainer.Register(Component.For<IHelper>().Instance(helper).LifestyleTransient());
					}
				}

				_providerFactory = _windsorContainer.Resolve<IProviderFactory>();
			}

			return new ProviderWrapper(_providerFactory.CreateProvider(identifer));
		}

		/// <summary>
		/// Gets the synchronizer in the app domain for the specific identifier
		/// </summary>
		/// <param name="identifier">The identifier that represents the synchronizer to create.</param>
		/// <param name="options">The options for that synchronizer that will be passed on initialization.</param>
		/// <returns>A synchronizer that will bring data into a system.</returns>
		public Synchronizer.IDataSynchronizer GetSynchronizer(Guid identifier, string options)
		{
			if (_windsorContainer == null)
			{
				this.SetUpCastleWindsor();
			}

			if (_synchronizerFactory == null)
			{
				_synchronizerFactory = _windsorContainer.Resolve<ISynchronizerFactory>();
			}

			return new SynchronizerWrapper(_synchronizerFactory.CreateSynchronizer(identifier, options));
		}
	}
}

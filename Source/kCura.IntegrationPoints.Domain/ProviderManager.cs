using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Castle.Windsor.Installer;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Contracts.Provider;
using Relativity.API;

namespace kCura.IntegrationPoints.Domain
{
	/// <summary>
	/// Entry point into the App domain to create the Provider
	/// Internal use only:
	/// This class should not be referenced by any consuming library
	///  </summary>
	public class DomainManager : MarshalByRefObject
	{
		//AK: perhaps the filter should be a little more generic to cover all related binaries  kCura.IntegrationPoints.*
		private static readonly HashSet<string> _allowedInstallerAssemblies = new HashSet<string>()
		{
			"kCura.IntegrationPoints",
			"kCura.IntegrationPoints.Contracts",
			"kCura.IntegrationPoints.Core",
			"kCura.IntegrationPoints.Data",
            "kCura.LDAPProvider",
            "kCura.IntegrationPoints.FtpProvider",
            "kCura.IntegrationPoints.ImportProvider.Parser"
		};

		private IProviderFactory _providerFactory;
		private WindsorContainer _windsorContainer;

	    public DomainManager()
	    {
	    }

		/// <summary>
		/// Called to initialized the provider's app domain and do any setup work needed
		/// </summary>
		public void Init()
		{
			System.Reflection.Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			Type startupType = typeof(IStartUp);
			Type providerFactoryType = typeof(IProviderFactory);

			var startupTypes = new List<Type>();
			Type customProviderFactoryType = null;
			foreach (var assembly in assemblies)
			{
				Type[] loadableTypes = assembly.GetLoadableTypes();
				foreach (Type type in loadableTypes)
				{
					if (startupType.IsAssignableFrom(type) && type != startupType)
					{
						startupTypes.Add(type);
					}
					else if (providerFactoryType.IsAssignableFrom(type) 
							&& type != typeof (DefaultProviderFactory) 
							&& type != typeof (ProviderFactoryBase)
							&& type != providerFactoryType)
					{
						if (customProviderFactoryType != null)
						{
							throw new Exception(Constants.IntegrationPoints.TOO_MANY_PROVIDER_FACTORIES);
						}

						customProviderFactoryType = type;
					}
				}
				startupTypes.AddRange(loadableTypes.Where(type => startupType.IsAssignableFrom(type) && type != startupType));
			}

			if (startupTypes.Any())
			{
				Type type = startupTypes.FirstOrDefault();
				if (type != default(Type))
				{
					var instance = Activator.CreateInstance(type) as IStartUp;
					instance?.Execute();
				}
			}

			if (customProviderFactoryType != null)
			{
				try
				{
					var customProviderFactory = Activator.CreateInstance(customProviderFactoryType) as IProviderFactory;
					_providerFactory = customProviderFactory;
				}
				catch (Exception ex)
				{
					throw new Exception(Constants.IntegrationPoints.UNABLE_TO_INSTANTIATE_PROVIDER_FACTORY, ex);	
				}
			}

			// Get marshaled data
			IAppDomainDataMarshaller dataMarshaller = new SecureAppDomainDataMarshaller();
			this.SetUpConnectionString(dataMarshaller);
		}

		/// <summary>
		/// Sets the connection string for the domain by retrieving the encrypted AppDomain data
		/// </summary>
		private void SetUpConnectionString(IAppDomainDataMarshaller dataMarshaller)
		{
			byte[] data = dataMarshaller.RetrieveMarshaledData(AppDomain.CurrentDomain, Constants.IntegrationPoints.APP_DOMAIN_DATA_CONNECTION_STRING);
			if (data != null && data.Length > 0)
			{
				string connectionString = System.Text.Encoding.ASCII.GetString(data);

				kCura.Config.Config.SetConnectionString(connectionString);
			}
		}

		private void SetUpCastleWindsor(IHelper helper)
		{
			_windsorContainer = new WindsorContainer();
			IKernel kernel = _windsorContainer.Kernel;
			kernel.Resolver.AddSubResolver(new CollectionResolver(kernel, true));

			_windsorContainer.Install(
				FromAssembly.InDirectory(
					new AssemblyFilter(AppDomain.CurrentDomain.BaseDirectory)
					.FilterByName(this.FilterByAllowedAssemblyNames)));
			_windsorContainer.Register(Component.For<IHelper>().UsingFactoryMethod(k => helper, true));
		}

		private bool FilterByAllowedAssemblyNames(AssemblyName assemblyName)
		{
			return _allowedInstallerAssemblies.Contains(assemblyName.Name);
		}


		/// <summary>
		/// Gets the provider in the app domain from the specific identifier.
		/// </summary>
		/// <param name="identifier">The identifier that represents the provider</param>
		/// <param name="helper">IHelper required in castle windsor initialization</param>
		/// <returns>A Data source provider to retrieve data and pass along to the source.</returns>
		public IDataSourceProvider GetProvider(Guid identifier, IHelper helper)
		{
			if (_windsorContainer == null)
			{
				this.SetUpCastleWindsor(helper);
			}

			if (_providerFactory == null)
			{
				try
				{
					_providerFactory = _windsorContainer.Resolve<IProviderFactory>();
				}
				catch
				{
					// Handle case (in event handlers) where IProviderFactory cannot be resolved...
					_providerFactory = new DefaultProviderFactory(_windsorContainer, helper);	
				}
			}

			return new ProviderWrapper(_providerFactory.CreateProvider(identifier));
		}
	}
}
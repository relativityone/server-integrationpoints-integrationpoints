using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Castle.Windsor.Installer;
using kCura.IntegrationPoints.Core.Domain;
using kCura.IntegrationPoints.Core.Services.Marshaller;
using Relativity.API;

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
		private WindsorContainer _windsorContainer;

		/// <summary>
		/// Called to initialized the provider's app domain and do any setup work needed
		/// </summary>
		public void Init()
		{
			// Resolve new app domain's assemblies
			AppDomain.CurrentDomain.AssemblyResolve += AssemblyDomainLoader.ResolveAssembly;
			System.Reflection.Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			Type startupType = typeof(IStartUp);

			var types = new List<Type>();
			foreach (var assembly in assemblies)
			{
				Type[] loadableTypes = assembly.GetLoadableTypes();
				types.AddRange(loadableTypes.Where(type => startupType.IsAssignableFrom(type) && type != startupType));
			}

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

			// Get marshaled data
			IAppDomainDataMarshaller dataMarshaller = new SecureAppDomainDataMarshaller();
			this.SetUpConnectionString(dataMarshaller);
		}

		/// <summary>
		/// Sets the connection string for the domain by retrieving the encrypted AppDomain data
		/// </summary>
		private void SetUpConnectionString(IAppDomainDataMarshaller dataMarshaller)
		{
			byte[] data = dataMarshaller.RetrieveMarshaledData(AppDomain.CurrentDomain, Core.Constants.IntegrationPoints.APP_DOMAIN_DATA_CONNECTION_STRING);
			if (data != null && data.Length > 0)
			{
				string connectionString = System.Text.Encoding.ASCII.GetString(data);

				kCura.Config.Config.SetConnectionString(connectionString);
			}
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
						_windsorContainer.Register(Component.For<IHelper>().Instance(helper).LifestyleSingleton());
					}
				}

				try
				{
					_providerFactory = _windsorContainer.Resolve<IProviderFactory>();
				}
				catch
				{
					// Handle case (in event handlers) where IProviderFactory cannot be resolved...
					_providerFactory = new DefaultProviderFactory(_windsorContainer);					
				}
			}

			return new ProviderWrapper(_providerFactory.CreateProvider(identifer));
		}
	}
}
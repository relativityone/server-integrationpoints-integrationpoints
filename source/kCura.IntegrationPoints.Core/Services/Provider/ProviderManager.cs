using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Reflection;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Castle.Windsor.Installer;
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

			Bootstrapper.InitAppDomain(Constants.IntegrationPoints.AppDomain_Subsystem_Name, Constants.IntegrationPoints.Application_GuidString, AppDomain.CurrentDomain);
			this.SetUpSystemToken();
			this.SetUpConnectionString();
		}

		private void SetUpSystemToken()
		{
			object systemTokenProvider = AppDomain.CurrentDomain.GetData(Constants.IntegrationPoints.AppDomain_Data_SystemTokenProvider);
			ExtensionPointServiceFinder.SystemTokenProvider = systemTokenProvider as IProvideSystemTokens;
		}

		private void SetUpConnectionString()
		{
			string connectionString = AppDomain.CurrentDomain.GetData(Constants.IntegrationPoints.AppDomain_Data_ConnectionString) as string;
			kCura.Config.Config.SetConnectionString(connectionString);
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

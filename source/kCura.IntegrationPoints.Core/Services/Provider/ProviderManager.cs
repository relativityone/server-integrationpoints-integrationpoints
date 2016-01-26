using System;
using System.Linq;
using System.Reflection;
using Castle.MicroKernel;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Castle.Windsor.Installer;
using kCura.IntegrationPoints.Core.Domain;

namespace kCura.IntegrationPoints.Contracts
{
	using System.Collections.Generic;

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
			List<Type> test = new List<Type>();
			try
			{
				foreach (Assembly assembly in assemblies)
				{
					Type[] test2 = assembly.GetTypes();
					foreach (var def in test2)
					{
						if (startupType.IsAssignableFrom(def) && def != startupType)
						{
							test.Add(def);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
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
		}

		private void SetUpCastleWindsor()
		{
			_windsorContainer = new WindsorContainer();
			IKernel kernel = _windsorContainer.Kernel;
			kernel.Resolver.AddSubResolver(new CollectionResolver(kernel, true));
			var installerFactory = new InstallerFactory();
			_windsorContainer.Install(FromAssembly.InThisApplication(installerFactory));
		}

		/// <summary>
		/// Gets the provider in the app domain from the specific identifier
		/// </summary>
		/// <param name="identifer">The identifier that represents the provider</param>
		/// <returns>A Data source provider to retrieve data and pass along to the source.</returns>
		public Provider.IDataSourceProvider GetProvider(Guid identifer)
		{
			if (_windsorContainer == null)
			{
				this.SetUpCastleWindsor();
			}

			if (_providerFactory == null)
			{
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
		public Synchronizer.IDataSynchronizer GetSyncronizer(Guid identifier, string options)
		{
			if (_windsorContainer == null)
			{
				this.SetUpCastleWindsor();
			}

			if (_synchronizerFactory == null)
			{
				_synchronizerFactory = _windsorContainer.Resolve<ISynchronizerFactory>();
			}

			return new SynchronizerWrapper(_synchronizerFactory.CreateSyncronizer(identifier, options));
		}

	}
}

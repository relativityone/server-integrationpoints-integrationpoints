using System;
using System.Linq;
using System.Reflection;
using kCura.IntegrationPoints.Contracts.Synchronizer;
using kCura.IntegrationPoints.Core.Domain;

namespace kCura.IntegrationPoints.Contracts
{
	/// <summary>
	/// Entry point into the App domain to create the Provider
	/// Internal use only:
	/// This class should not be referenced by any consuming library
	///  </summary>
	public class DomainManager : MarshalByRefObject
	{
		/// <summary>
		/// Called to initilized the provider's app domain and do any setup work needed
		/// </summary>
		public void Init()
		{
			AppDomain.CurrentDomain.AssemblyResolve += AssemblyDomainLoader.ResolveAssembly;
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			var startupType = typeof(IStartUp);
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

		/// <summary>
		/// Gets the provider in the app domain from the specific identifier
		/// </summary>
		/// <param name="identifer">The identifier that represents the provider</param>
		/// <returns>A Data source provider to retrieve data and pass along to the source.</returns>
		public Provider.IDataSourceProvider GetProvider(Guid identifer)
		{
			return new ProviderWrapper(PluginBuilder.Current.GetProviderFactory().CreateProvider(identifer));
		}

		/// <summary>
		/// Gets the synchronizer in the app domain for the specific identifier
		/// </summary>
		/// <param name="identifier">The identifier that represents the synchronizer to create.</param>
		/// <param name="options">The options for that synchronizer that will be passed on initialization.</param>
		/// <returns>A synchronizer that will bring data into a system.</returns>
		public IDataSynchronizer GetSyncronizer(Guid identifier, string options)
		{
			return new SynchronizerWrapper(PluginBuilder.Current.GetSynchronizerFactory().CreateSyncronizer(identifier, options));
		}

	}
}

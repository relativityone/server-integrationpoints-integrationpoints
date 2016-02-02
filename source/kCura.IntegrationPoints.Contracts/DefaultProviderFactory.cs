using System;
using System.Linq;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.Contracts
{
	/// <summary>
	/// Represents a default provider used to create data source providers.
	/// </summary>
	public class DefaultProviderFactory : IProviderFactory
	{
		private readonly IWindsorContainer _windsorContainer;

		/// <summary>
        ///Initializes an new instance of the DefaultProviderFactory class.
        /// </summary>     
		/// <param name="windsorContainer">The windsorContainer from which to resolve providers</param>
        public DefaultProviderFactory(IWindsorContainer windsorContainer)
		{
			_windsorContainer = windsorContainer;
		}

		/// <summary>
		/// Creates a new data source provider using the GUID specified as the identifier.
		/// </summary>
		/// <param name="identifier"> A GUID decorating the class so that it can be accessed.</param>
        /// <returns>A new instance of a data source provider.</returns>
		public IDataSourceProvider CreateProvider(Guid identifier)
		{
			Type t = GetType(identifier);
			return CreateInstance(t);
		}
		/// <summary>
		/// Gets the Type of the current instance.
		/// </summary>
		/// <param name="identifer"></param>
		/// <returns></returns>
		protected virtual Type GetType(Guid identifer)
		{
			var types = from a in AppDomain.CurrentDomain.GetAssemblies()
									from t in a.GetTypes()
									where t.IsDefined(typeof(DataSourceProviderAttribute), true) 
									select t;
			var providerTypes = types.Where(x => x.GetCustomAttributes(typeof(DataSourceProviderAttribute), true)
													.Cast<DataSourceProviderAttribute>().Any(y => y.Identifier.Equals(identifer))).ToList();
			if (providerTypes.Count() > 1)
			{
				throw new Exception(string.Format(Properties.Resources.MoreThanOneProviderFound, providerTypes.Count(), identifer));
			}
			if (!providerTypes.Any())
			{
				throw new Exception(string.Format(Properties.Resources.NoProvidersFound, identifer));
			}
			return providerTypes.First();
		}

		/// <summary>
        /// Creates a new instance of the provider type using the Activator with an empty constructor.
		/// </summary>
		/// <param name="providerType">The type of the provider to create.</param>
        /// <returns>A new instance of a data source provider.</returns>
		protected virtual IDataSourceProvider CreateInstance(Type providerType)
		{
			IDataSourceProvider provider = null;
			string assemblyQualifiedName = providerType.AssemblyQualifiedName;

			try
			{
				if (!_windsorContainer.Kernel.HasComponent(assemblyQualifiedName))
				{
					_windsorContainer.Register(
						Component.For<IDataSourceProvider>().ImplementedBy(providerType).Named(assemblyQualifiedName));
				}

				provider = _windsorContainer.Resolve<IDataSourceProvider>(assemblyQualifiedName);
			} 
			catch
			{
				throw new Exception(string.Format(Properties.Resources.CouldNotCreateProvider, providerType));
			}

			// TODO: check if provider can be null -- biedrzycki: Jan 25, 2016
			return provider;
		}
	}
}

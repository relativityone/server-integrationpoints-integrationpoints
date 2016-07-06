using System;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.Domain
{
	/// <summary>
	/// Represents a default provider used to create data source providers.
	/// </summary>
	internal class DefaultProviderFactory : ProviderFactoryBase
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
        /// Creates a new instance of the provider type using the Activator with an empty constructor.
		/// </summary>
		/// <param name="providerType">The type of the provider to create.</param>
        /// <returns>A new instance of a data source provider.</returns>
		public override IDataSourceProvider CreateInstance(Type providerType)
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
			catch (Exception ex)
			{
			    throw new Exception(string.Format(Contracts.Properties.Resources.CouldNotCreateProvider, providerType));
			}

			// TODO: check if provider can be null -- biedrzycki: Jan 25, 2016
			return provider;
		}
	}
}

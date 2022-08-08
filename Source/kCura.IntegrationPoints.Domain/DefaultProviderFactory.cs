using System;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Domain.Exceptions;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Properties;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.IntegrationPoints.SourceProviderInstaller;
using Relativity.IntegrationPoints.SourceProviderInstaller.Internals.Wrappers;

namespace kCura.IntegrationPoints.Domain
{
    /// <summary>
    /// Represents a default provider used to create data source providers.
    /// </summary>
    public class DefaultProviderFactory : ProviderFactoryBase
    {
        private readonly IWindsorContainer _windsorContainer;
        private readonly IAPILog _logger;

        ///  <summary>
        /// Initializes an new instance of the DefaultProviderFactory class.
        ///  </summary>
        /// <param name="windsorContainer">The windsorContainer from which to resolve providers</param>
        /// <param name="helper"></param>
        public DefaultProviderFactory(IWindsorContainer windsorContainer, IHelper helper)
        {
            _windsorContainer = windsorContainer;
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<DefaultProviderFactory>();
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
            catch(Exception ex)
            {
                var message = string.Format(Resources.CouldNotCreateProvider, providerType);
                LogProviderCreationError(ex, message);
                throw new IntegrationPointsException(message);
            }

            return new CrossAppDomainProviderWrapper(provider);
        }

#region Logging

        private void LogProviderCreationError(Exception ex, string message)
        {
            _logger.LogError(ex, "Could not create provider. Details: {Message}", message);
        }

#endregion
    }
}

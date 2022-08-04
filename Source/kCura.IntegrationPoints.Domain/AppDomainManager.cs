using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Windsor;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts;
using Relativity.IntegrationPoints.SourceProviderInstaller;
using Relativity.IntegrationPoints.SourceProviderInstaller.Internals;

namespace kCura.IntegrationPoints.Domain
{
    /// <summary>
    /// Entry point into the App domain to create the Provider
    /// Internal use only:
    /// This class should not be referenced by any consuming library
    ///  </summary>
    public class AppDomainManager : MarshalByRefObject, IAppDomainManager
    {
        private IProviderFactory _customProviderFactory;
        private readonly WindsorContainerSetup _windsorContainerSetup;
        private readonly IHelper _helper;

        public AppDomainManager(IHelper helper)
        {
            _helper = helper;
            _windsorContainerSetup = new WindsorContainerSetup();
        }

        /// <summary>
        /// Called to initialized the provider's app domain and do any setup work needed
        /// </summary>
        public void Init()
        {
            CreateCustomProviderFactory();
        }

        private void CreateCustomProviderFactory()
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
                            && type != typeof(DefaultProviderFactory)
                            && type != typeof(ProviderFactoryBase)
                            && type != typeof(InternalProviderFactory)
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
                    _customProviderFactory = customProviderFactory;
                }
                catch (Exception ex)
                {
                    throw new Exception(Constants.IntegrationPoints.UNABLE_TO_INSTANTIATE_PROVIDER_FACTORY, ex);
                }
            }
        }

        public IProviderFactory CreateProviderFactory()
        {
            IWindsorContainer container = _windsorContainerSetup.SetUpCastleWindsor(_helper);
            IProviderFactory providerFactory = _customProviderFactory;
            if (_customProviderFactory == null)
            {
                try
                {
                    providerFactory = container.Resolve<IProviderFactory>();
                }
                catch
                {
                    providerFactory = new DefaultProviderFactory(container, _helper);
                }
            }

            return providerFactory;
        }
    }
}
using System;
using kCura.IntegrationPoints.Contracts.Provider;

namespace JsonLoader
{
	// Required for non-default constructor in our provider
    public class DIProviderFactory : kCura.IntegrationPoints.Contracts.ProviderFactoryBase
    {
        public override IDataSourceProvider CreateInstance(Type providerType)
        {
            return new JsonProvider(new JsonHelper());
        }
    }
}
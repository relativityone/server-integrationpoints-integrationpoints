using System;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Domain;

namespace JsonLoader
{
	// Required for non-default constructor in our provider
    public class DIProviderFactory : ProviderFactoryBase
    {
        public override IDataSourceProvider CreateInstance(Type providerType)
        {
            return new JsonProvider(new JsonHelper());
        }
    }
}
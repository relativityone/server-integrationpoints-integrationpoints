using System;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.IntegrationPoints.SourceProviderInstaller;

namespace Relativity.IntegrationPoints.JsonLoader
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
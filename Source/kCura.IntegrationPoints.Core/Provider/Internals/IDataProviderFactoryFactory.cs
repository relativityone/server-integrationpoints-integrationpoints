using kCura.IntegrationPoints.Core.Services.Domain;
using kCura.IntegrationPoints.Core.Services.Provider;
using LanguageExt;

namespace kCura.IntegrationPoints.Core.Provider.Internals
{
    public interface IDataProviderFactoryFactory
    {
        Either<string, ProviderFactoryVendor> CreateProviderFactoryVendor();

        IDataProviderFactory CreateDataProviderFactory(ProviderFactoryVendor providerFactoryVendor);
    }
}

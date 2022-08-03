using Relativity.IntegrationPoints.Contracts;

namespace kCura.IntegrationPoints.Domain
{
    public interface IAppDomainManager
    {
        /// <summary>
        /// Called to initialized the provider's app domain and do any setup work needed
        /// </summary>
        void Init();

        IProviderFactory CreateProviderFactory();
    }
}
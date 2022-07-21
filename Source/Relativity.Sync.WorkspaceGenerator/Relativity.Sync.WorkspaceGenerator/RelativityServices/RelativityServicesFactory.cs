using Relativity.Services.ServiceProxy;
using Relativity.Sync.WorkspaceGenerator.Settings;

namespace Relativity.Sync.WorkspaceGenerator.RelativityServices
{
    public class RelativityServicesFactory : IRelativityServicesFactory
    {
        private readonly ServiceFactory _serviceFactory;

        public RelativityServicesFactory(GeneratorSettings settings)
        {
            var credentials = new UsernamePasswordCredentials(settings.RelativityUserName, settings.RelativityPassword);
            var serviceFactorySettings = new ServiceFactorySettings(settings.RelativityServicesUri, settings.RelativityRestApiUri, credentials);
            _serviceFactory = new ServiceFactory(serviceFactorySettings);
        }

        public IWorkspaceService CreateWorkspaceService()
        {
            return new WorkspaceService(_serviceFactory);
        }

        public ISavedSearchManager CreateSavedSearchManager()
        {
            return new SavedSearchManager(_serviceFactory);
        }

        public IProductionService CreateProductionService()
        {
            return new ProductionService(_serviceFactory);
        }
    }
}
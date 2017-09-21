using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Web.Toggles;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Data
{
    public class ResourcePoolContext : IResourcePoolContext
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IToggleProvider _toggleProvider;

        public ResourcePoolContext(IRepositoryFactory repositoryFactory, IToggleProvider toggleProvider)
        {
            _repositoryFactory = repositoryFactory;
            _toggleProvider = toggleProvider;
        }

        private bool IsCloudInstance()
        {
            IInstanceSettingRepository instanceSettings = _repositoryFactory.GetInstanceSettingRepository();
            string cloudInstanceName = instanceSettings.GetConfigurationValue(Domain.Constants.RELATIVITY_CORE_SECTION, Domain.Constants.CLOUD_INSTANCE_NAME);
            return !string.IsNullOrWhiteSpace(cloudInstanceName);
        }

        public bool IsProcessingSourceLocationEnabled()
        {
            return _toggleProvider.IsEnabled<ProcessingSourceLocationToggle>() && !IsCloudInstance();
        }
    }
}
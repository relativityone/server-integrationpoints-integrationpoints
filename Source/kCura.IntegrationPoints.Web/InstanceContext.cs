using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Web
{
    public class InstanceContext : IInstanceContext
    {
        private readonly IRepositoryFactory _repositoryFactory;

        public InstanceContext(IRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public bool IsCloudInstance()
        {
            IInstanceSettingRepository instanceSettings = _repositoryFactory.GetInstanceSettingRepository();
            string cloudInstanceName = instanceSettings.GetConfigurationValue(Domain.Constants.RELATIVITY_CORE_SECTION, Domain.Constants.CLOUD_INSTANCE_NAME);
            return !string.IsNullOrWhiteSpace(cloudInstanceName);
        }
    }
}
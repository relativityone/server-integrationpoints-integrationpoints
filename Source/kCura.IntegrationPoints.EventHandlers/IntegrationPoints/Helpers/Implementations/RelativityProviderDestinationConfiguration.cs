using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Contracts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;
using Artifact = kCura.EventHandler.Artifact;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations
{
    public class RelativityProviderDestinationConfiguration : RelativityProviderConfiguration
    {
        private readonly IFederatedInstanceManager _federatedInstanceManager;
        private readonly IRepositoryFactory _repositoryFactory;
        private const string ARTIFACT_TYPE_NAME = "ArtifactTypeName";
        private const string DESTINATION_RELATIVITY_INSTANCE = "DestinationRelativityInstance";

        public RelativityProviderDestinationConfiguration(IEHHelper helper, IFederatedInstanceManager federatedInstanceManager, IRepositoryFactory repositoryFactory)
            : base(helper)
        {
            _federatedInstanceManager = federatedInstanceManager;
            _repositoryFactory = repositoryFactory;
        }

        public override void UpdateNames(IDictionary<string, object> settings, Artifact artifact)
        {
            SetArtifactTypeName(settings);
            SetDestinationInstanceName(settings);
        }

        private void SetArtifactTypeName(IDictionary<string, object> settings)
        {
            try
            {
                int transferredObjArtifactTypeId = ParseValue<int>(settings,
                    nameof(DestinationConfiguration.ArtifactTypeId));

                IObjectTypeRepository objectTypeRepository = _repositoryFactory.GetObjectTypeRepository(Helper.GetActiveCaseID());
                ObjectTypeDTO objectType = objectTypeRepository.GetObjectType(transferredObjArtifactTypeId);
                settings[ARTIFACT_TYPE_NAME] = objectType.Name;
            }
            catch (Exception ex)
            {
                Helper.GetLoggerFactory().GetLogger().LogError(ex, "Cannot retrieve object type name for artifact type id");
                settings[ARTIFACT_TYPE_NAME] = "RDO";
            }
        }

        private void SetDestinationInstanceName(IDictionary<string, object> settings)
        {
            int federatedInstanceArtifactId = ParseValue<int>(settings,
                nameof(DestinationConfiguration.FederatedInstanceArtifactId));
            if (federatedInstanceArtifactId != default(int))
            {
                FederatedInstanceDto federatedInstanceDto = _federatedInstanceManager.RetrieveFederatedInstanceByArtifactId(federatedInstanceArtifactId);
                settings[DESTINATION_RELATIVITY_INSTANCE] = federatedInstanceDto.Name;
            }
        }
    }
}

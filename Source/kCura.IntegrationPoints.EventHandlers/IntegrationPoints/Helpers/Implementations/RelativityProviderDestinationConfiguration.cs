using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.API;
using Artifact = kCura.EventHandler.Artifact;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations
{
    public class RelativityProviderDestinationConfiguration : RelativityProviderConfiguration
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private const string ARTIFACT_TYPE_NAME = "ArtifactTypeName";

        public RelativityProviderDestinationConfiguration(IEHHelper helper, IRepositoryFactory repositoryFactory)
            : base(helper)
        {
            _repositoryFactory = repositoryFactory;
        }

        public override void UpdateNames(IDictionary<string, object> settings, Artifact artifact)
        {
            SetArtifactTypeName(settings);
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
    }
}

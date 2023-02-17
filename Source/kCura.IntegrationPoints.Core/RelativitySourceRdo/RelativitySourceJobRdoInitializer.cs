using System;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.RelativitySourceRdo
{
    public class RelativitySourceJobRdoInitializer : IRelativitySourceJobRdoInitializer
    {
        private readonly IAPILog _logger;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IRelativitySourceRdoHelpersFactory _helpersFactory;
        private readonly Guid _sourceJobObjectTypeGuid = SourceJobDTO.ObjectTypeGuid;
        private readonly string _sourceJobObjectTypeName = IntegrationPoints.Domain.Constants.SPECIAL_SOURCEJOB_FIELD_NAME;
        private readonly Guid _sourceJobFieldOnDocumentGuid = SourceJobDTO.Fields.JobHistoryFieldOnDocumentGuid;

        public RelativitySourceJobRdoInitializer(IHelper helper, IRepositoryFactory repositoryFactory, IRelativitySourceRdoHelpersFactory helpersFactory)
        {
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<RelativitySourceJobRdoInitializer>();
            _repositoryFactory = repositoryFactory;
            _helpersFactory = helpersFactory;
        }

        public int InitializeWorkspaceWithSourceJobRdo(int destinationWorkspaceArtifactId, int sourceWorkspaceArtifactTypeId)
        {
            try
            {
                ISourceJobRepository sourceJobRepository = _repositoryFactory.GetSourceJobRepository(destinationWorkspaceArtifactId);

                IRelativitySourceRdoObjectType relativitySourceRdoObjectType = _helpersFactory.CreateRelativitySourceRdoObjectType(sourceJobRepository);
                IRelativitySourceRdoDocumentField relativitySourceRdoDocumentField = _helpersFactory.CreateRelativitySourceRdoDocumentField(sourceJobRepository);
                IRelativitySourceRdoFields relativitySourceRdoFields = _helpersFactory.CreateRelativitySourceRdoFields();

                int sourceJobDescriptorArtifactTypeId = relativitySourceRdoObjectType.CreateObjectType(destinationWorkspaceArtifactId, _sourceJobObjectTypeGuid,
                    _sourceJobObjectTypeName, sourceWorkspaceArtifactTypeId);
                relativitySourceRdoFields.CreateFields(destinationWorkspaceArtifactId, SourceJobDTO.Fields.GetFieldsDefinition(sourceJobDescriptorArtifactTypeId));
                relativitySourceRdoDocumentField.CreateDocumentField(destinationWorkspaceArtifactId, _sourceJobFieldOnDocumentGuid, _sourceJobObjectTypeName, sourceJobDescriptorArtifactTypeId);
                return sourceJobDescriptorArtifactTypeId;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to create Relativity Source Job object.");
                throw new Exception("Unable to create Relativity Source Job object. Please contact the system administrator.", e);
            }
        }
    }
}

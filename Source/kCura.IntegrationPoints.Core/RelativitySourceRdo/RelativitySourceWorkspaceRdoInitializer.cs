using System;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using Relativity;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.RelativitySourceRdo
{
    public class RelativitySourceWorkspaceRdoInitializer : IRelativitySourceWorkspaceRdoInitializer
    {
        private readonly IAPILog _logger;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IRelativitySourceRdoHelpersFactory _helpersFactory;
        private readonly Guid _sourceWorkspaceObjectTypeGuid = SourceWorkspaceDTO.ObjectTypeGuid;
        private readonly string _sourceWorkspaceObjectTypeName = Domain.Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME;
        private readonly Guid _sourceWorkspaceFieldOnDocumentGuid = SourceWorkspaceDTO.Fields.SourceWorkspaceFieldOnDocumentGuid;

        public RelativitySourceWorkspaceRdoInitializer(IHelper helper, IRepositoryFactory repositoryFactory, IRelativitySourceRdoHelpersFactory helpersFactory)
        {
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<RelativitySourceWorkspaceRdoInitializer>();
            _repositoryFactory = repositoryFactory;
            _helpersFactory = helpersFactory;
        }

        public int InitializeWorkspaceWithSourceWorkspaceRdo(int destinationWorkspaceArtifactId)
        {
            try
            {
                ISourceWorkspaceRepository sourceWorkspaceRepository = _repositoryFactory.GetSourceWorkspaceRepository(destinationWorkspaceArtifactId);
                int sourceWorkspaceDescriptorArtifactTypeId = GetWorkspaceDescriptorArtifactTypeId(destinationWorkspaceArtifactId, sourceWorkspaceRepository);

                CreateSourceRdoFields(destinationWorkspaceArtifactId, sourceWorkspaceDescriptorArtifactTypeId);
                CreateSourceDocumentFields(destinationWorkspaceArtifactId, sourceWorkspaceRepository, sourceWorkspaceDescriptorArtifactTypeId);

                return sourceWorkspaceDescriptorArtifactTypeId;
            }
            catch (Exception e)
            {
                throw LogAndWrapException(e);
            }
        }

        private int GetWorkspaceDescriptorArtifactTypeId(int destinationWorkspaceArtifactId, ISourceWorkspaceRepository sourceWorkspaceRepository)
        {
            IRelativitySourceRdoObjectType relativitySourceRdoObjectType = _helpersFactory.CreateRelativitySourceRdoObjectType(sourceWorkspaceRepository);
            int sourceWorkspaceDescriptorArtifactTypeId = relativitySourceRdoObjectType.CreateObjectType(destinationWorkspaceArtifactId, _sourceWorkspaceObjectTypeGuid,
                _sourceWorkspaceObjectTypeName, (int)ArtifactType.Case);
            return sourceWorkspaceDescriptorArtifactTypeId;
        }

        private void CreateSourceRdoFields(int destinationWorkspaceArtifactId, int sourceWorkspaceDescriptorArtifactTypeId)
        {
            IRelativitySourceRdoFields relativitySourceRdoFields = _helpersFactory.CreateRelativitySourceRdoFields();
            relativitySourceRdoFields.CreateFields(destinationWorkspaceArtifactId, SourceWorkspaceDTO.Fields.GetFieldsDefinition(sourceWorkspaceDescriptorArtifactTypeId));
        }

        private void CreateSourceDocumentFields(int destinationWorkspaceArtifactId, ISourceWorkspaceRepository sourceWorkspaceRepository, int sourceWorkspaceDescriptorArtifactTypeId)
        {
            IRelativitySourceRdoDocumentField relativitySourceRdoDocumentField = _helpersFactory.CreateRelativitySourceRdoDocumentField(sourceWorkspaceRepository);
            relativitySourceRdoDocumentField.CreateDocumentField(destinationWorkspaceArtifactId, _sourceWorkspaceFieldOnDocumentGuid, _sourceWorkspaceObjectTypeName,
                sourceWorkspaceDescriptorArtifactTypeId);
        }

        private IntegrationPointsException LogAndWrapException(Exception e)
        {
            string message = "Unable to create Relativity Source Case object. Please contact the system administrator.";
            _logger.LogError(e, message);
            return new IntegrationPointsException(message, e)
            {
                ShouldAddToErrorsTab = true
            };
        }
    }
}
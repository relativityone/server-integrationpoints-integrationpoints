using System;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.RelativitySourceRdo
{
	public class RelativitySourceWorkspaceRdoInitializer : IRelativitySourceWorkspaceRdoInitializer
	{
		private readonly IAPILog _logger;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IRelativitySourceRdoHelpersFactory _helpersFactory;
		private readonly Guid _sourceWorkspaceObjectTypeGuid = SourceWorkspaceDTO.ObjectTypeGuid;
		private readonly string _sourceWorkspaceObjectTypeName = IntegrationPoints.Domain.Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME;
		private readonly Guid _sourceWorkspaceFieldOnDocumentGuid = SourceWorkspaceDTO.Fields.SourceWorkspaceFieldOnDocumentGuid;

		public RelativitySourceWorkspaceRdoInitializer(IHelper helper, IRepositoryFactory repositoryFactory, IRelativitySourceRdoHelpersFactory helpersFactory)
		{
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<RelativitySourceWorkspaceRdoInitializer>();
			_repositoryFactory = repositoryFactory;
			_helpersFactory = helpersFactory;
		}

		public int InitializeWorkspaceWithSourceWorkspaceRdo(int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId)
		{
			try
			{
				ISourceWorkspaceRepository sourceWorkspaceRepository = _repositoryFactory.GetSourceWorkspaceRepository(destinationWorkspaceArtifactId);

				IRelativitySourceRdoObjectType relativitySourceRdoObjectType = _helpersFactory.CreateRelativitySourceRdoObjectType(sourceWorkspaceRepository);
				IRelativitySourceRdoDocumentField relativitySourceRdoDocumentField = _helpersFactory.CreateRelativitySourceRdoDocumentField(sourceWorkspaceRepository);
				IRelativitySourceRdoFields relativitySourceRdoFields = _helpersFactory.CreateRelativitySourceRdoFields();

				int sourceWorkspaceDescriptorArtifactTypeId = relativitySourceRdoObjectType.CreateObjectType(destinationWorkspaceArtifactId, _sourceWorkspaceObjectTypeGuid,
					_sourceWorkspaceObjectTypeName, (int) ArtifactType.Case);
				relativitySourceRdoFields.CreateFields(destinationWorkspaceArtifactId, SourceWorkspaceDTO.Fields.GetFieldsDefinition(sourceWorkspaceDescriptorArtifactTypeId));
				relativitySourceRdoDocumentField.CreateDocumentField(destinationWorkspaceArtifactId, _sourceWorkspaceFieldOnDocumentGuid, _sourceWorkspaceObjectTypeName,
					sourceWorkspaceDescriptorArtifactTypeId);
				return sourceWorkspaceDescriptorArtifactTypeId;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Unable to create Relativity Source Case object.");
				throw new Exception("Unable to create Relativity Source Case object. Please contact the system administrator.", e);
			}
		}
	}
}
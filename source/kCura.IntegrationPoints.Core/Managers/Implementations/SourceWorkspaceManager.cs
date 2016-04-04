using System;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class SourceWorkspaceManager : ISourceWorkspaceManager
	{
		private readonly ISourceWorkspaceRepository _sourceWorkspaceRepository;

		public SourceWorkspaceManager(ISourceWorkspaceRepository sourceWorkspaceRepository)
		{
			_sourceWorkspaceRepository = sourceWorkspaceRepository;
		}

		public SourceWorkspaceFieldMapDTO InititializeWorkspace(int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId)
		{
			int? sourceWorkspaceArtifactTypeId = _sourceWorkspaceRepository.RetrieveObjectTypeDescriptorArtifactTypeId(destinationWorkspaceArtifactId);
			if (!sourceWorkspaceArtifactTypeId.HasValue)
			{
				sourceWorkspaceArtifactTypeId = _sourceWorkspaceRepository.CreateObjectType(destinationWorkspaceArtifactId);	
			}

			if (!_sourceWorkspaceRepository.ObjectTypeFieldExist(destinationWorkspaceArtifactId,
				sourceWorkspaceArtifactTypeId.Value))
			{
				_sourceWorkspaceRepository.CreateObjectTypeFields(destinationWorkspaceArtifactId, sourceWorkspaceArtifactTypeId.Value);	
			}

			int sourceWorkspaceFieldOnDocument = -1;
			try
			{
				sourceWorkspaceFieldOnDocument = _sourceWorkspaceRepository.GetSourceWorkspaceFieldOnDocument(destinationWorkspaceArtifactId, sourceWorkspaceArtifactTypeId.Value);
			}
			catch
			{
				sourceWorkspaceFieldOnDocument = _sourceWorkspaceRepository.CreateSourceWorkspaceFieldOnDocument(destinationWorkspaceArtifactId, sourceWorkspaceArtifactTypeId.Value);
			}

			string destinationWorkspaceName = "THIS IS A TEST"; // TODO: get the workspace name
			SourceWorkspaceDTO sourceWorkspaceDto = _sourceWorkspaceRepository.RetrieveForSourceWorkspaceId(destinationWorkspaceArtifactId, sourceWorkspaceArtifactId, sourceWorkspaceArtifactTypeId.Value);
			if (sourceWorkspaceDto == null)
			{
				sourceWorkspaceDto = new SourceWorkspaceDTO()
				{
					ArtifactId = -1,
					Name = String.Format("{0} - {1}", destinationWorkspaceName, sourceWorkspaceArtifactId),
					SourceWorkspaceArtifactId = sourceWorkspaceArtifactId,
					SourceWorkspaceName = destinationWorkspaceName
				};
				int artifactId = _sourceWorkspaceRepository.Create(sourceWorkspaceArtifactId, sourceWorkspaceArtifactTypeId.Value, sourceWorkspaceDto);

				sourceWorkspaceDto.ArtifactId = artifactId;
			}

			// TODO: check if the workspace name has changed and update if so

			var sourceWorkspaceFieldMap = new SourceWorkspaceFieldMapDTO()
			{
				SourceWorkspaceDto = sourceWorkspaceDto,
				SourceWorkspaceDocumentFieldArtifactId = sourceWorkspaceFieldOnDocument
			};

			return sourceWorkspaceFieldMap;
		}
	}
}
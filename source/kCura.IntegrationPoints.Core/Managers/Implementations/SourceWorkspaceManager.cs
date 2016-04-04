using System;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class SourceWorkspaceManager : ISourceWorkspaceManager
	{
		private readonly ISourceWorkspaceRepository _sourceWorkspaceRepository;
		private readonly IWorkspaceRepository _workspaceRepository;

		public SourceWorkspaceManager(ISourceWorkspaceRepository sourceWorkspaceRepository, IWorkspaceRepository workspaceRepository)
		{
			_sourceWorkspaceRepository = sourceWorkspaceRepository;
			_workspaceRepository = workspaceRepository;
		}

		public SourceWorkspaceDTO InititializeWorkspace(int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId)
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

			try
			{
				_sourceWorkspaceRepository.GetSourceWorkspaceFieldOnDocument(destinationWorkspaceArtifactId, sourceWorkspaceArtifactTypeId.Value);
			}
			catch
			{
				_sourceWorkspaceRepository.CreateSourceWorkspaceFieldOnDocument(destinationWorkspaceArtifactId, sourceWorkspaceArtifactTypeId.Value);
			}

			WorkspaceDTO workspaceDto = _workspaceRepository.Retrieve(destinationWorkspaceArtifactId);
			SourceWorkspaceDTO sourceWorkspaceDto = _sourceWorkspaceRepository.RetrieveForSourceWorkspaceId(destinationWorkspaceArtifactId, sourceWorkspaceArtifactId, sourceWorkspaceArtifactTypeId.Value);
			if (sourceWorkspaceDto == null)
			{
				sourceWorkspaceDto = new SourceWorkspaceDTO()
				{
					ArtifactId = -1,
					Name = String.Format("{0} - {1}", workspaceDto.Name, sourceWorkspaceArtifactId),
					SourceWorkspaceArtifactId = sourceWorkspaceArtifactId,
					SourceWorkspaceName = workspaceDto.Name
				};
				int artifactId = _sourceWorkspaceRepository.Create(sourceWorkspaceArtifactId, sourceWorkspaceArtifactTypeId.Value, sourceWorkspaceDto);

				sourceWorkspaceDto.ArtifactId = artifactId;
			}

			// TODO: check if the workspace name has changed and update if so

			return sourceWorkspaceDto;
		}
	}
}
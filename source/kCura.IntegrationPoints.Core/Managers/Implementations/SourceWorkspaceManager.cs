using System;
using System.Collections.Generic;
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

		public void InititializeWorkspace(int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId)
		{
			int? sourceWorkspaceArtifactTypeId = _sourceWorkspaceRepository.RetrieveObjectTypeDescriptorArtifactTypeId(destinationWorkspaceArtifactId);
			if (!sourceWorkspaceArtifactTypeId.HasValue)
			{
				sourceWorkspaceArtifactTypeId = _sourceWorkspaceRepository.CreateObjectType(destinationWorkspaceArtifactId);	
			}

			IDictionary<string, int> fieldNameToArtifactDictionary = null;
			try
			{
				fieldNameToArtifactDictionary = _sourceWorkspaceRepository.GetObjectTypeFieldArtifactIds(destinationWorkspaceArtifactId,
					sourceWorkspaceArtifactTypeId.Value);
			}
			catch
			{
				fieldNameToArtifactDictionary = _sourceWorkspaceRepository.CreateObjectTypeFields(destinationWorkspaceArtifactId,
					sourceWorkspaceArtifactTypeId.Value);
			}

			int sourceWorkspaceFieldOnDocument = 0;
			try
			{
				sourceWorkspaceFieldOnDocument = _sourceWorkspaceRepository.GetSourceWorkspaceFieldOnDocument(destinationWorkspaceArtifactId, sourceWorkspaceArtifactTypeId.Value);
			}
			catch
			{
				sourceWorkspaceFieldOnDocument = _sourceWorkspaceRepository.CreateSourceWorkspaceFieldOnDocument(destinationWorkspaceArtifactId, sourceWorkspaceArtifactTypeId.Value);
			}

			SourceWorkspaceDTO sourceWorkspaceDto = _sourceWorkspaceRepository.RetrieveForSourceWorkspaceId(destinationWorkspaceArtifactId, sourceWorkspaceArtifactId,
				sourceWorkspaceArtifactId, fieldNameToArtifactDictionary);

			if (sourceWorkspaceDto == null)
			{
				sourceWorkspaceDto = new SourceWorkspaceDTO()
				{
					ArtifactId = -1,
					Name = "THIS IS A TEST",
					SourceWorkspaceArtifactId = sourceWorkspaceArtifactId
				};
				int artifactId = _sourceWorkspaceRepository.Create(sourceWorkspaceArtifactId, sourceWorkspaceArtifactTypeId.Value, sourceWorkspaceDto, fieldNameToArtifactDictionary);

				sourceWorkspaceDto.ArtifactId = artifactId;
			}
		}
	}
}
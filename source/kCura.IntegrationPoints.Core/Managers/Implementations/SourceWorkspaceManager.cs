using System;
using System.Collections.Generic;
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

		public void InititializeWorkspace(int workspaceArtifactId)
		{
			int? sourceWorkspaceArtifactTypeId = _sourceWorkspaceRepository.RetrieveObjectTypeDescriptorArtifactTypeId(workspaceArtifactId);
			if (!sourceWorkspaceArtifactTypeId.HasValue)
			{
				sourceWorkspaceArtifactTypeId = _sourceWorkspaceRepository.CreateObjectType(workspaceArtifactId);	
			}

			IDictionary<string, int> fieldNameToArtifactDictionary = null;
			try
			{
				fieldNameToArtifactDictionary = _sourceWorkspaceRepository.GetObjectTypeFieldArtifactIds(workspaceArtifactId,
					sourceWorkspaceArtifactTypeId.Value);
			}
			catch
			{
				fieldNameToArtifactDictionary = _sourceWorkspaceRepository.CreateObjectTypeFields(workspaceArtifactId,
					sourceWorkspaceArtifactTypeId.Value);
			}
		}
	}
}
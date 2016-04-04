using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface ISourceWorkspaceRepository
	{
		int? RetrieveObjectTypeDescriptorArtifactTypeId(int workspaceArtifactId);
		int CreateObjectType(int workspaceArtifactId);

		SourceWorkspaceDTO RetrieveForSourceWorkspaceId(int workspaceArtifactId, int sourceWorkspaceArtifactId,
			int sourceWorkspaceArtifactTypeId, IDictionary<string, int> fieldNameToIdDictionary);
		int Create(int workspsaceArtifactId, int sourceWorkspaceArtifactTypeId, SourceWorkspaceDTO sourceWorkspaceDto, IDictionary<string, int> fieldNameToIdDictionary);

		IDictionary<string, int> GetObjectTypeFieldArtifactIds(int workspaceArtifactId, int sourceWorkspaceObjectTypeId);
		IDictionary<string, int> CreateObjectTypeFields(int workspaceArtifactId, int sourceWorkspaceObjectTypeId);

		int CreateSourceWorkspaceFieldOnDocument(int workspaceArtifactId, int sourceWorkspaceObjectTypeId);
		int GetSourceWorkspaceFieldOnDocument(int workspaceArtifactId, int sourceWorkspaceObjectTypeId);
	}
}
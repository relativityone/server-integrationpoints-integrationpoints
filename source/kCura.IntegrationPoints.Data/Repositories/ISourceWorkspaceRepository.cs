using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface ISourceWorkspaceRepository
	{
		int? RetrieveObjectTypeDescriptorArtifactTypeId(int workspaceArtifactId);
		int CreateObjectType(int workspaceArtifactId);
		SourceWorkspaceDTO Retrieve(int workspaceArtifactId);
		int Create(int workspaceArtifactId, SourceWorkspaceDTO sourceWorkspaceDto);

		IDictionary<string, int> GetObjectTypeFieldArtifactIds(int workspaceArtifactId, int sourceWorkspaceObjectTypeId);
		IDictionary<string, int> CreateObjectTypeFields(int workspaceArtifactId, int sourceWorkspaceObjectTypeId);
	}
}
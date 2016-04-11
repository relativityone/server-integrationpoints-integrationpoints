using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface ISourceWorkspaceRepository
	{
		int CreateObjectType();

		SourceWorkspaceDTO RetrieveForSourceWorkspaceId(int sourceWorkspaceArtifactId);
		int Create(int sourceWorkspaceArtifactTypeId, SourceWorkspaceDTO sourceWorkspaceDto);

		IDictionary<Guid, int> CreateObjectTypeFields(int sourceWorkspaceObjectTypeId, IEnumerable<Guid> fieldGuids);

		int CreateSourceWorkspaceFieldOnDocument(int sourceWorkspaceObjectTypeId);
		bool SourceWorkspaceFieldExistsOnDocument(int sourceWorkspaceObjectTypeId, out int fieldArtifactId);
		void Update(SourceWorkspaceDTO sourceWorkspaceDto);
		int? RetrieveTabArtifactId(int sourceWorkspaceArtifactTypeId);
		void DeleteTab(int tabArtifactId);
	}
}
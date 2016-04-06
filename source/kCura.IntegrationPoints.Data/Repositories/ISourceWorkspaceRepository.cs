using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface ISourceWorkspaceRepository
	{
		int? RetrieveObjectTypeDescriptorArtifactTypeId(int workspaceArtifactId);
		int CreateObjectType(int workspaceArtifactId);

		SourceWorkspaceDTO RetrieveForSourceWorkspaceId(int workspaceArtifactId, int sourceWorkspaceArtifactId, int sourceWorkspaceArtifactTypeId);
		int Create(int workspaceArtifactId, int sourceWorkspaceArtifactTypeId, SourceWorkspaceDTO sourceWorkspaceDto);

		bool ObjectTypeFieldsExist(int workspaceArtifactId, int sourceWorkspaceObjectTypeId);
		void CreateObjectTypeFields(int workspaceArtifactId, int sourceWorkspaceObjectTypeId);

		int CreateSourceWorkspaceFieldOnDocument(int workspaceArtifactId, int sourceWorkspaceObjectTypeId);
		bool SourceWorkspaceFieldExistsOnDocument(int workspaceArtifactId, int sourceWorkspaceObjectTypeId);
		void Update(int workspaceArtifactId, SourceWorkspaceDTO sourceWorkspaceDto);
	}
}
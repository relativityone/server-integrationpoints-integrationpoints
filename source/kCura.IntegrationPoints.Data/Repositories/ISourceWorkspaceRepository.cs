using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface ISourceWorkspaceRepository
	{
		int? RetrieveObjectTypeDescriptorArtifactTypeId();
		int CreateObjectType();

		SourceWorkspaceDTO RetrieveForSourceWorkspaceId(int sourceWorkspaceArtifactId, int sourceWorkspaceArtifactTypeId);
		int Create(int sourceWorkspaceArtifactTypeId, SourceWorkspaceDTO sourceWorkspaceDto);

		bool ObjectTypeFieldsExist(int sourceWorkspaceObjectTypeId);
		void CreateObjectTypeFields(int sourceWorkspaceObjectTypeId);

		int CreateSourceWorkspaceFieldOnDocument(int sourceWorkspaceObjectTypeId);
		bool SourceWorkspaceFieldExistsOnDocument(int sourceWorkspaceObjectTypeId);
		void Update(SourceWorkspaceDTO sourceWorkspaceDto);
	}
}
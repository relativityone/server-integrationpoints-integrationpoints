using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface ITargetWorkspaceJobHistoryRepository
	{
		int? RetrieveObjectTypeDescriptorArtifactTypeId(int workspaceArtifactId);
		int CreateObjectType(int workspaceArtifactId, int sourceWorkspaceArtifactTypeId);
		int Create(int workspaceArtifactId, int jobHistoryArtifactTypeId, TargetWorkspaceJobHistoryDTO targetWorkspaceJobHistoryDto);
		bool ObjectTypeFieldsExist(int workspaceArtifactId, int jobHistoryArtifactTypeId);
		void CreateObjectTypeFields(int workspaceArtifactId, int jobHistoryArtifactTypeId);
		int CreateJobHistoryFieldOnDocument(int workspaceArtifactId, int jobHistoryArtifactTypeId);
		int GetJobHistoryFieldOnDocument(int workspaceArtifactId, int jobHistoryArtifactTypeId);
	}
}
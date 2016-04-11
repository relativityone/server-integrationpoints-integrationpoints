using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface ITargetWorkspaceJobHistoryRepository
	{
		int? RetrieveObjectTypeDescriptorArtifactTypeId();
		int CreateObjectType(int sourceWorkspaceArtifactTypeId);
		int Create(int jobHistoryArtifactTypeId, TargetWorkspaceJobHistoryDTO targetWorkspaceJobHistoryDto);
		bool ObjectTypeFieldsExist(int jobHistoryArtifactTypeId);
		int CreateJobHistoryFieldOnDocument(int jobHistoryArtifactTypeId);
		bool JobHistoryFieldExistsOnDocument(int jobHistoryArtifactTypeId);
		IDictionary<Guid, int> CreateObjectTypeFields(int jobHistoryArtifactTypeId, IEnumerable<Guid> fieldGuids);
	}
}
using System;
using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IObjectTypeRepository
	{
		int? RetrieveObjectTypeDescriptorArtifactTypeId(Guid objectTypeGuid);
		void Delete(int artifactId);
	}
}
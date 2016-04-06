using System;
using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IObjectTypeRepository
	{
		bool GetObjectTypeExists(Guid objectTypeGuid);
		void Create(ObjectTypeDTO objectTypeDto);
	}
}
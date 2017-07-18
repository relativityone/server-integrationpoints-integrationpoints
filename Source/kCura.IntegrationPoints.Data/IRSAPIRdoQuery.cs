using System.Collections.Generic;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data
{
	public interface IRsapiRdoQuery : IObjectTypeQuery
	{
		List<ObjectType> GetAllRdo(List<int> typeIds = null);

		ObjectType GetObjectType(int typeId);

		ObjectType GetType(int typeId);

		int GetObjectTypeID(string objectTypeName);
	}
}

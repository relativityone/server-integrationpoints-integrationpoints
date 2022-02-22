using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Data
{
	public interface IObjectTypeQuery
	{
		List<ObjectTypeDTO> GetAllTypes(int userId);
	}
}

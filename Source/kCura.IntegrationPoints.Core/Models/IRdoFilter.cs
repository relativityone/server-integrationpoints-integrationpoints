using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Models
{
	public interface IRdoFilter
	{
		IEnumerable<ObjectTypeDTO> GetAllViewableRdos();
	}
}

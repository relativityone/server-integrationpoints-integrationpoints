using System.Collections.Generic;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Core.Models
{
	public interface IRdoFilter
	{
		IEnumerable<ObjectType> GetAllViewableRdos();
	}
}

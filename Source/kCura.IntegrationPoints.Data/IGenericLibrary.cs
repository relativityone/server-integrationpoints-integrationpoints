using System.Collections.Generic;

namespace kCura.IntegrationPoints.Data
{
	public interface IGenericLibrary<T> where T: BaseRdo, new()
	{
		List<int> Create(IEnumerable<T> rdos);
	}
}

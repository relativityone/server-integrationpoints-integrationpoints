using System.Collections.Generic;

namespace kCura.IntegrationPoints.Data
{
	public interface IGenericLibrary<T> where T: BaseRdo, new()
	{
		List<int> Create(IEnumerable<T> rdos);
		bool Update(IEnumerable<T> rdos);
		bool Delete(IEnumerable<int> artifactIds);
		bool Delete(IEnumerable<T> rdos);
	}
}

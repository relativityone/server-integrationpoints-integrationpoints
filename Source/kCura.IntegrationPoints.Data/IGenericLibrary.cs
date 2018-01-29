using System.Collections.Generic;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data
{
	public interface IGenericLibrary<T> where T: BaseRdo, new()
	{
		List<int> Create(IEnumerable<T> rdos);
		bool Update(IEnumerable<T> objs);
		bool Delete(IEnumerable<int> artifactIds);
		bool Delete(IEnumerable<T> objs);
		void MassDelete(IEnumerable<T> objs);
		MassCreateResult MassCreate(IEnumerable<T> objs);
		MassEditResult MassEdit(IEnumerable<T> objs);
	}
}

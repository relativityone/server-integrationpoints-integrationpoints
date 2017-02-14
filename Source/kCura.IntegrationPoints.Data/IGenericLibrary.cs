using System.Collections.Generic;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data
{
	public interface IGenericLibrary<T> where T: BaseRdo, new()
	{
		int Create(T integrationPoint);
		List<int> Create(IEnumerable<T> integrationPoints);
		T Read(int artifactId);
		List<T> Read(IEnumerable<int> artifactIds);
		bool Update(T obj);
		bool Update(IEnumerable<T> objs);
		bool Delete(int artifactId);
		bool Delete(IEnumerable<int> artifactIds);
		bool Delete(T obj);
		bool Delete(IEnumerable<T> objs);
		void MassDelete(IEnumerable<T> objs);
		MassCreateResult MassCreate(IEnumerable<T> objs);
		MassEditResult MassEdit(IEnumerable<T> objs);
		List<T> Query(Query<RDO> q, int pageSize = 0);
	}
}

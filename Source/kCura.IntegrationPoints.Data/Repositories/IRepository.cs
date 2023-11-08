using System.Collections.Generic;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IRepository<T> where T : BaseRdo, new()
	{
		IEnumerable<T> GetAll(ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser);

		int Create(T rdo, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser);

		bool Update(T rdo, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser);

		bool Delete(T rdo, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser);
	}
}

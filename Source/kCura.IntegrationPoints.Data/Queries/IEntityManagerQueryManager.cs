using kCura.IntegrationPoints.Data.Factories;
using Relativity.API;
using System.Data;

namespace kCura.IntegrationPoints.Data.Queries
{
	public interface IEntityManagerQueryManager
	{
		ICommand CreateEntityManagerResourceTable(IRepositoryFactory repositoryFactory,
			IDBContext caseDBcontext, string tableName, int workspaceID);

		ICommand InsertDataToEntityManagerResourceTable(IRepositoryFactory repositoryFactory,
			IDBContext caseDBcontext, string tableName, DataTable entityManagerRows, int workspaceID);

		IQuery<DataTable> GetJobEntityManagerLinks(IRepositoryFactory repositoryFactory,
			IDBContext caseDBcontext, string tableName, long jobID, int workspaceID);

		IQuery<DataTable> GetRelatedJobsCountWithTaskTypeExceptions(IDBContext eddsDBcontext, long rootJobID, string[] taskTypeExceptions);
	}
}

using kCura.IntegrationPoints.Data.Factories;
using Relativity.API;
using System.Data;

namespace kCura.IntegrationPoints.Data.Queries
{
	public class EntityManagerQueryManager : IEntityManagerQueryManager
	{
		public ICommand CreateEntityManagerResourceTable(IRepositoryFactory repositoryFactory, IDBContext caseDBcontext, string tableName, int workspaceID)
		{
			return new CreateEntityManagerResourceTable(repositoryFactory, caseDBcontext, tableName, workspaceID);
		}

		public IQuery<DataTable> GetJobEntityManagerLinks(IRepositoryFactory repositoryFactory, IDBContext caseDBcontext, string tableName, long jobID, int workspaceID)
		{
			return new GetJobEntityManagerLinks(repositoryFactory, caseDBcontext, tableName, jobID, workspaceID);
		}

		public IQuery<DataTable> GetRelatedJobsCountWithTaskTypeExceptions(IDBContext eddsDBcontext, long rootJobID, string[] taskTypeExceptions)
		{
			return new GetJobsCount(eddsDBcontext, rootJobID, taskTypeExceptions);
		}

		public ICommand InsertDataToEntityManagerResourceTable(IRepositoryFactory repositoryFactory, IDBContext caseDBcontext, string tableName, DataTable entityManagerRows, int workspaceID)
		{
			return new InsertDataToEntityManagerResourceTable(repositoryFactory, caseDBcontext, tableName, entityManagerRows, workspaceID);
		}

		public ICommand UnlockEntityManagerLinks(IRepositoryFactory repositoryFactory, IDBContext caseDBcontext, string tableName, long jobID, int workspaceID)
		{
			return new UnlockEntityManagerLinks(repositoryFactory, caseDBcontext, tableName, jobID, workspaceID);
		}

	}
}

using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Queries
{
	public class CreateEntityManagerResourceTable
	{
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IDBContext _caseDBcontext;

		public CreateEntityManagerResourceTable(IRepositoryFactory repositoryFactory, IDBContext caseDBcontext)
		{
			_repositoryFactory = repositoryFactory;
			_caseDBcontext = caseDBcontext;
		}

		public void Execute(string tableName, int workspaceID)
		{
			IScratchTableRepository scratchTableRepository = _repositoryFactory.GetScratchTableRepository(workspaceID, string.Empty, string.Empty);
			string sql = string.Format(Resources.Resource.CreateEntityManagerResourceTable,
				scratchTableRepository.GetResourceDBPrepend(), scratchTableRepository.GetSchemalessResourceDataBasePrepend(), tableName);
			_caseDBcontext.ExecuteNonQuerySQLStatement(sql);
		}
	}
}
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Queries
{
	public class CreateEntityManagerResourceTable : ICommand
	{
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IDBContext _caseDBcontext;
		private readonly string _tableName;
		private readonly int _workspaceID;

		public CreateEntityManagerResourceTable(IRepositoryFactory repositoryFactory, 
			IDBContext caseDBcontext, string tableName, int workspaceID)
		{
			_repositoryFactory = repositoryFactory;
			_caseDBcontext = caseDBcontext;
			_tableName = tableName;
			_workspaceID = workspaceID;
		}

		public void Execute()
		{
			IScratchTableRepository scratchTableRepository = _repositoryFactory.GetScratchTableRepository(_workspaceID, string.Empty, string.Empty);
			string sql = string.Format(Resources.Resource.CreateEntityManagerResourceTable,
				scratchTableRepository.GetResourceDBPrepend(), scratchTableRepository.GetSchemalessResourceDataBasePrepend(), _tableName);
			_caseDBcontext.ExecuteNonQuerySQLStatement(sql);
		}
	}
}
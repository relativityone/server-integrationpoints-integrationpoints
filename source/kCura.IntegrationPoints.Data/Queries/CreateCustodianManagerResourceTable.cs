using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Queries
{
    public class CreateCustodianManagerResourceTable
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IDBContext _caseDBcontext;

        public CreateCustodianManagerResourceTable(IRepositoryFactory repositoryFactory, IDBContext caseDBcontext)
        {
            _repositoryFactory = repositoryFactory;
            _caseDBcontext = caseDBcontext;
        }

        public void Execute(string tableName, int workspaceID)
        {
            IScratchTableRepository scratchTableRepository = _repositoryFactory.GetScratchTableRepository(workspaceID, string.Empty, string.Empty);
            var sql = string.Format(Resources.Resource.CreateCustodianManagerResourceTable,
                scratchTableRepository.GetResourceDBPrepend(), scratchTableRepository.GetSchemalessResourceDataBasePrepend(), tableName);
            _caseDBcontext.ExecuteNonQuerySQLStatement(sql);
        }
    }
}
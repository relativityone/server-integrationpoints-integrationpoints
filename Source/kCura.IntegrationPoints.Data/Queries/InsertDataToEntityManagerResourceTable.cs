using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.API;
using System.Data;
using System.Data.SqlClient;

namespace kCura.IntegrationPoints.Data.Queries
{
    public class InsertDataToEntityManagerResourceTable : ICommand
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IDBContext _caseDBcontext;
        private readonly string _tableName;
        private readonly DataTable _entityManagerRows;
        private readonly int _workspaceID;

        public InsertDataToEntityManagerResourceTable(IRepositoryFactory repositoryFactory,
            IDBContext caseDBcontext, string tableName, DataTable entityManagerRows, int workspaceID)
        {
            _repositoryFactory = repositoryFactory;
            _caseDBcontext = caseDBcontext;
            _tableName = tableName;
            _entityManagerRows = entityManagerRows;
            _workspaceID = workspaceID;
        }

        public void Execute()
        {
            IScratchTableRepository scratchTableRepository = _repositoryFactory.GetScratchTableRepository(_workspaceID, string.Empty, string.Empty);
            using (SqlBulkCopy sbc = new SqlBulkCopy(_caseDBcontext.GetConnection()))
            {
                sbc.DestinationTableName = string.Format("{0}.[{1}]", scratchTableRepository.GetResourceDBPrepend(), _tableName);

                // Map the Source Column from DataTabel to the Destination Columns
                sbc.ColumnMappings.Add("EntityID", "EntityID");
                sbc.ColumnMappings.Add("ManagerID", "ManagerID");
                sbc.ColumnMappings.Add("CreatedOn", "CreatedOn");

                // Finally write to server
                sbc.WriteToServer(_entityManagerRows);
                sbc.Close();
            }
        }
    }
}

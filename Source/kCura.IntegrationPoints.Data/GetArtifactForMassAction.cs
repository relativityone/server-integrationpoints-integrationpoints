using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Data
{
    public class GetArtifactForMassAction
    {
        private readonly IRepositoryFactory _repositoryFactory;
        public GetArtifactForMassAction(IRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }
        public List<Int32> GetArtifactsToBeDeleted(IWorkspaceDBContext workspaceContext, String tempTableName, int workspaceID)
        {
            IScratchTableRepository scratchTableRepository = _repositoryFactory.GetScratchTableRepository(workspaceID, string.Empty, string.Empty);
            //create a sql statement which will select the list of ArtifactIDs from the TempTableNameWithParentArtifactsToDelete scratch table
            string sql = string.Format("SELECT [ArtifactID] FROM {0}.[{1}]", scratchTableRepository.GetResourceDBPrepend(), tempTableName);
            //get the artifact ids from the table and convert to a generic list of Int32
            return workspaceContext.ExecuteSqlStatementAsDataTable(sql).Rows.Cast<System.Data.DataRow>().Select(dr => Convert.ToInt32(dr["ArtifactID"])).ToList();
        }
    }
}
